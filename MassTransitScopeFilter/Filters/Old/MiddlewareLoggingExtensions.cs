using GreenPipes;
using MassTransit;

namespace MassTransitScopeFilter.Filters.Old
{
    public static class MiddlewareLoggingExtensions
    {
        public static void UseLoggerInterceptor<T>(this IPipeConfigurator<T> configurator)
            where T : class, ConsumeContext
        {
            configurator.AddPipeSpecification(new LoggerSpecification<T>());
        }
    }
}