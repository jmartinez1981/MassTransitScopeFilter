using System;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransitScopeFilter.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;

namespace MassTransitScopeFilter.Filters.Old
{
    public class LoggerFilter<T> : IFilter<T>
        where T : class, ConsumeContext
    {
        public void Probe(ProbeContext context)
        {
            // Do nothing because the actual logic is within Send method.
            // this method is defined on an interface used to gain information about the bus.
        }

        public async Task Send(T context, IPipe<T> next)
        {
            var conversationId = GetSystemCorrelationId(context);

            using (LogContext.PushProperty("ConversationId", conversationId))
            {
                SetContext(context, conversationId);
                await next.Send(context).ConfigureAwait(false);
            }
        }

        private static Guid GetSystemCorrelationId(T context)
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

        private static void SetContext(T context, Guid conversationId)
        {
            context.GetPayload<IServiceScope>()
                .ServiceProvider
                .GetService<IScopedCache>()
                .ConversationId = conversationId;
        }
    }
}