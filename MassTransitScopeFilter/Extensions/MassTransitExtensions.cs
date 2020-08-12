using MassTransit;
using MassTransitScopeFilter.Filters.New;
using MassTransitScopeFilter.Filters.Old;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MassTransitScopeFilter.Extensions
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection ConfigureMassTransit(
            this IServiceCollection services,
            IConfiguration configuration,
            string hostname,
            int port,
            string virtualHost,
            string user,
            string pwd)
        {
            var isScopedFilter = configuration.GetValue<bool>("settings:scopedFilter");

            if (isScopedFilter)
            {
                services.AddScoped(typeof(ScopedLoggerFilter<>));
            }

            services.AddHostedService<MassTransitHostedService>();

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri($"rabbitmq://{hostname}:{port}/{virtualHost}"), hostCfg =>
                    {
                        hostCfg.Username(user);
                        hostCfg.Password(pwd);
                    });

                    if (isScopedFilter)
                    {
                        cfg.UseConsumeFilter(typeof(ScopedLoggerFilter<>), context);
                    }
                    else
                    {
                        cfg.UseServiceScope(context.GetRequiredService<IServiceProvider>());
                        cfg.UseLoggerInterceptor();
                    }
                    
                    cfg.ConfigureEndpoints(context);
                });
                x.AddConsumer<TestConsumer>();
            });

            return services;
        }
    }
}
