﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Marten;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence
{
    public class MartenEnvelopePersistor : IEnvelopePersistor
    {
        private readonly IDocumentStore _store;
        private readonly EnvelopeTables _tables;

        public MartenEnvelopePersistor(IDocumentStore store, EnvelopeTables tables)
        {
            _store = store;
            _tables = tables;
        }

        public async Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            using (var session = _store.LightweightSession())
            {
                session.DeleteEnvelopes(_tables.Incoming, envelopes);
                await session.SaveChangesAsync();
            }
        }

        public async Task DeleteOutgoingEnvelopes(Envelope[] envelopes)
        {
            using (var session = _store.LightweightSession())
            {
                session.DeleteEnvelopes(_tables.Outgoing, envelopes);
                await session.SaveChangesAsync();
            }
        }

        public async Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            using (var session = _store.LightweightSession())
            {
                session.Store(errors);
                session.DeleteEnvelopes(_tables.Incoming, errors.Select(x => x.Id).ToArray());
                await session.SaveChangesAsync();
            }
        }

        public async Task ScheduleExecution(Envelope[] envelopes)
        {
            using (var session = _store.LightweightSession())
            {
                foreach (var envelope in envelopes)
                {
                    session.ScheduleExecution(_tables.Incoming, envelope);
                }

                await session.SaveChangesAsync();
            }
        }

        public async Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            using (var session = _store.QuerySession())
            {
                return await session.LoadAsync<ErrorReport>(id);
            }
        }

        public async Task IncrementIncomingEnvelopeAttempts(Envelope envelope)
        {
            using (var conn = _store.Tenancy.Default.CreateConnection())
            {
                await conn.OpenAsync();

                await conn.CreateCommand($"update {_tables.Incoming} set attempts = :attempts where id = :id")
                    .With("attempts", envelope.Attempts, NpgsqlDbType.Integer)
                    .With("id", envelope.Id, NpgsqlDbType.Uuid)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task StoreIncoming(Envelope envelope)
        {
            using (var session = _store.LightweightSession())
            {
                session.StoreIncoming(_tables, envelope);
                await session.SaveChangesAsync();
            }
        }

        public async Task StoreIncoming(IEnumerable<Envelope> envelopes)
        {
            using (var session = _store.LightweightSession())
            {
                foreach (var envelope in envelopes)
                {
                    session.StoreIncoming(_tables, envelope);
                }

                await session.SaveChangesAsync();
            }
        }
    }
}