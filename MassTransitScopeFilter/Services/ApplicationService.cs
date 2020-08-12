using MassTransit;
using MassTransitScopeFilter.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MassTransitScopeFilter.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IBusControl publishEndpoint;
        private readonly IScopedCache scopedCache;
        private readonly ILogger<ApplicationService> logger;

        public ApplicationService(
            IBusControl publishEndpoint,
            IScopedCache scopedCache,
            ILogger<ApplicationService> logger)
        {
            this.publishEndpoint = publishEndpoint;
            this.scopedCache = scopedCache;
            this.logger = logger;
        }

        public async Task PublishCorrelatingMessage()
        {
            this.logger.LogInformation($"The correlationId from scopedCache is: [{this.scopedCache.ConversationId}].");

            var msg = new MessageB();

            await this.publishEndpoint.Publish(msg, msg.GetType(), context =>
            {
                context.ConversationId = this.scopedCache.ConversationId;
            }).ConfigureAwait(false);
        }
    }
}
