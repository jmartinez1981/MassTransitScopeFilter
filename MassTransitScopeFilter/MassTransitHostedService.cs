using MassTransit;
using MassTransitScopeFilter.Contracts;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransitScopeFilter
{
    public class MassTransitHostedService : IHostedService
    {
        private readonly IBusControl busControl;

        public MassTransitHostedService(IBusControl busControl)
        {
            this.busControl = busControl;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.busControl.StartAsync(cancellationToken).ConfigureAwait(false);
            await this.busControl.Publish(new MessageA()).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.busControl.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
