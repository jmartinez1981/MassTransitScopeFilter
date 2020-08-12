using GreenPipes;
using MassTransit;
using MassTransitScopeFilter.Services;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace MassTransitScopeFilter.Filters.New
{
    public class ScopedLoggerFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
    {
        private readonly IScopedCache scopedCache;

        public ScopedLoggerFilter(IScopedCache scopedCache) 
        {
            this.scopedCache = scopedCache;
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next) 
        {
            var conversationId = GetSystemCorrelationId(context);

            using (LogContext.PushProperty("ConversationId", conversationId))
            {
                SetContext(conversationId);
                await next.Send(context).ConfigureAwait(false);
            }
        }

        public void Probe(ProbeContext context) { }

        private static Guid GetSystemCorrelationId(ConsumeContext<T> context)
        {
            // maintaining correlation across devices.
            if (context.Headers != null &&
                context.Headers.TryGetHeader("correlation-id", out var correlationIdHeader) &&
                Guid.TryParse(correlationIdHeader.ToString(), out var correlationId) &&
                correlationId != Guid.Empty)
            {
                return correlationId;
            }

            // maintaining correlation for internal communication.
            if (context.ConversationId != null &&
                context.ConversationId.Value != Guid.Empty)
            {
                return context.ConversationId.Value;
            }

            // starting a new conversation.
            return Guid.NewGuid();
        }

        private void SetContext(Guid conversationId)
        {
            this.scopedCache.ConversationId = conversationId;
        }
    }
}
