using System.Threading.Tasks;

namespace MassTransitScopeFilter.Services
{
    public interface IApplicationService
    {
        Task PublishCorrelatingMessage();
    }
}
