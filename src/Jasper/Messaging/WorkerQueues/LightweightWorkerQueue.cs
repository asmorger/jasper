using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.WorkerQueues
{
    public class LightweightWorkerQueue : IWorkerQueue
    {
        private readonly ITransportLogger _logger;
        private readonly AdvancedSettings _settings;
        private readonly ActionBlock<Envelope> _receiver;
        private readonly InMemoryScheduledJobProcessor _scheduler;
        private IListener _agent;

        public LightweightWorkerQueue(ListenerSettings listenerSettings, ITransportLogger logger,
            IHandlerPipeline pipeline, AdvancedSettings settings)
        {
            _logger = logger;
            _settings = settings;
            Pipeline = pipeline;

            _scheduler = new InMemoryScheduledJobProcessor(this);

            listenerSettings.ExecutionOptions.CancellationToken = settings.Cancellation;

            _receiver = new ActionBlock<Envelope>(async envelope =>
            {
                try
                {
                    envelope.ContentType = envelope.ContentType ?? "application/json";

                    await Pipeline.Invoke(envelope);
                }
                catch (Exception e)
                {
                    // This *should* never happen, but of course it will
                    logger.LogException(e);
                }
            }, listenerSettings.ExecutionOptions);
        }

        public IHandlerPipeline Pipeline { get; }

        public int QueuedCount => _receiver.InputCount;
        public Task Enqueue(Envelope envelope)
        {
            if (envelope.IsPing()) return Task.CompletedTask;

            envelope.Callback = new LightweightCallback(this);
            _receiver.Post(envelope);

            return Task.CompletedTask;
        }

        public Task ScheduleExecution(Envelope envelope)
        {
            if (!envelope.ExecutionTime.HasValue) throw new ArgumentOutOfRangeException(nameof(envelope), $"There is no {nameof(Envelope.ExecutionTime)} value");

            _scheduler.Enqueue(envelope.ExecutionTime.Value, envelope);
            return Task.CompletedTask;
        }


        public void StartListening(IListener agent)
        {
            _agent = agent;
            _agent.Start(this);
        }

         public Uri Address => _agent.Address;

        public ListeningStatus Status
        {
            get => _agent.Status;
            set => _agent.Status = value;
        }

        async Task<ReceivedStatus> IReceiverCallback.Received(Uri uri, Envelope[] messages)
        {
            var now = DateTime.UtcNow;

            return await ProcessReceivedMessages(now, uri, messages);
        }

        Task IReceiverCallback.Acknowledged(Envelope[] messages)
        {
            return Task.CompletedTask;
        }

        Task IReceiverCallback.NotAcknowledged(Envelope[] messages)
        {
            return Task.CompletedTask;
        }

        Task IReceiverCallback.Failed(Exception exception, Envelope[] messages)
        {
            _logger.LogException(new MessageFailureException(messages, exception));
            return Task.CompletedTask;
        }


        public void Dispose()
        {
            // nothing
        }

        // Separated for testing here.
        public async Task<ReceivedStatus> ProcessReceivedMessages(DateTime now, Uri uri, Envelope[] envelopes)
        {
            if (_settings.Cancellation.IsCancellationRequested) return ReceivedStatus.ProcessFailure;

            try
            {
                Envelope.MarkReceived(envelopes, uri, DateTime.UtcNow, _settings.UniqueNodeId, out var scheduled, out var incoming);


                foreach (var envelope in scheduled)
                {
                    _scheduler.Enqueue(envelope.ExecutionTime.Value, envelope);
                }


                foreach (var message in incoming)
                {
                    await Enqueue(message);
                }

                _logger.IncomingBatchReceived(envelopes);

                return ReceivedStatus.Successful;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return ReceivedStatus.ProcessFailure;
            }
        }
    }
}
