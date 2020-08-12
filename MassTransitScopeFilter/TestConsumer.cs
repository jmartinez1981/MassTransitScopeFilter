using MassTransit;
using MassTransitScopeFilter.Contracts;
using MassTransitScopeFilter.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MassTransitScopeFilter
{
    public class TestConsumer : IConsumer<MessageA>
    {
        private readonly IScopedCache scopedCache;
        private readonly ILogger<TestConsumer> logger;

        public TestConsumer(IScopedCache scopedCache, ILogger<TestConsumer> logger)
        {
            this.logger = logger;
            this.scopedCache = scopedCache;
        }

        public async Task Consume(ConsumeContext<MessageA> context)
        {
            this.logger.LogInformation($"The correlationId from scopedCache is: [{this.scopedCache.ConversationId}].");

            var msg = new MessageB();

            await context.Publish(msg, msg.GetType(), context =>
            {
                context.ConversationId = this.scopedCache.ConversationId;
            }).ConfigureAwait(false);
        }
    }
}
