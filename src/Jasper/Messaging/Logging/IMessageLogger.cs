﻿using System;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Logging
{
    [CacheResolver]
    // SAMPLE: IMessageLogger
    public interface IMessageLogger
    {
        /// <summary>
        /// Catch all hook for any exceptions encountered by the messaging
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="correlationId"></param>
        /// <param name="message"></param>
        void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:");

        /// <summary>
        /// Called when an envelope is successfully sent through a transport
        /// </summary>
        /// <param name="envelope"></param>
        void Sent(Envelope envelope);

        /// <summary>
        /// Called when an envelope is first received by the current application
        /// </summary>
        /// <param name="envelope"></param>
        void Received(Envelope envelope);

        /// <summary>
        /// Marks the beginning of message execution
        /// </summary>
        /// <param name="envelope"></param>
        void ExecutionStarted(Envelope envelope);

        /// <summary>
        /// Marks the end of message execution
        /// </summary>
        /// <param name="envelope"></param>
        void ExecutionFinished(Envelope envelope);

        /// <summary>
        /// Called when a message has been successfully processed
        /// </summary>
        /// <param name="envelope"></param>
        void MessageSucceeded(Envelope envelope);

        /// <summary>
        /// Called when message execution has failed
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="ex"></param>
        void MessageFailed(Envelope envelope, Exception ex);

        /// <summary>
        /// Called when a message is received for which the application has no handler
        /// </summary>
        /// <param name="envelope"></param>
        void NoHandlerFor(Envelope envelope);

        /// <summary>
        /// Called when a Jasper application tries to send a message but cannot determine
        /// any subscribers or matching publishing rules
        /// </summary>
        /// <param name="envelope"></param>
        void NoRoutesFor(Envelope envelope);

        /// <summary>
        /// Called when Jasper moves an envelope into the dead letter queue
        /// </summary>
        /// <param name="envelope"></param>
        void MovedToErrorQueue(Envelope envelope, Exception ex);

        /// <summary>
        /// Called when Jasper discards a received envelope that has expired
        /// </summary>
        /// <param name="envelope"></param>
        void DiscardedEnvelope(Envelope envelope);
    }
    // ENDSAMPLE
}
