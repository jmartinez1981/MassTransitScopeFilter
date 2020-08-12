using MassTransit;
using MassTransit.Testing;
using MassTransitScopeFilter.Contracts;
using MassTransitScopeFilter.Filters.New;
using MassTransitScopeFilter.Filters.Old;
using MassTransitScopeFilter.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MassTransitScopeFilter.Tests
{
    public class FilterTests
    {
        [Fact]
        public async Task SendMessageA_Should_Publish_MessageB_WithCorrelationId_OldLogger()
        {
            var services = new ServiceCollection();

            services.AddSingleton(provider =>
            {
                var harness = new InMemoryTestHarness();
                harness.TestInactivityTimeout = new TimeSpan(0, 0, 20);

                var busRegistrationContext = provider.GetRequiredService<IBusRegistrationContext>();

                harness.OnConfigureInMemoryBus += configurator =>
                {
                    configurator.UseServiceScope(provider);
                    configurator.UseLoggerInterceptor();
                    configurator.ConfigureEndpoints(busRegistrationContext);
                };

                return harness;
            });

            services.AddMassTransit(cfg =>
            {
                cfg.AddBus(context => context.GetRequiredService<InMemoryTestHarness>().BusControl);
            });

            services.AddLogging(cfg => cfg.AddDebug());
            services.AddScoped<IScopedCache, ScopedCache>();
            services.AddScoped<IConsumer<MessageA>, TestConsumer>();

            var serviceProvider = services.BuildServiceProvider();

            var harness = serviceProvider.GetRequiredService<InMemoryTestHarness>();
            var consumerHarness = harness.Consumer(() =>
            {
                return serviceProvider.GetRequiredService<IConsumer<MessageA>>();
            });

            var correlationId = Guid.NewGuid();
            
            await harness.Start().ConfigureAwait(false);
            try
            {
                await harness.InputQueueSendEndpoint.Send(new MessageA(), context => 
                {
                    context.CorrelationId = correlationId;
                }).ConfigureAwait(false);

                Assert.True(await harness.Consumed.Any<MessageA>());

                Assert.True(await consumerHarness.Consumed.Any<MessageA>());

                Assert.True(await harness.Published.Any<MessageB>());

                // ensure that no faults were published by the consumer
                Assert.False(await harness.Published.Any<Fault<MessageB>>());

                var publishedMessage = harness.Published.Select<MessageB>().SingleOrDefault();
                Assert.Equal(correlationId, publishedMessage.Context.CorrelationId);
            }
            finally
            {
                await harness.Stop();
            }

        }

        [Fact]
        public async Task SendMessageA_Should_Publish_MessageB_WithCorrelationId_NewLogger()
        {
            var services = new ServiceCollection();

            services.AddSingleton(provider =>
            {
                var harness = new InMemoryTestHarness();
                harness.TestInactivityTimeout = new TimeSpan(0, 0, 20);

                var busRegistrationContext = provider.GetRequiredService<IBusRegistrationContext>();

                harness.OnConfigureInMemoryBus += configurator =>
                {
                    configurator.UseConsumeFilter(typeof(ScopedLoggerFilter<>), busRegistrationContext);
                    configurator.ConfigureEndpoints(busRegistrationContext);
                };

                return harness;
            });

            services.AddMassTransit(cfg =>
            {
                cfg.AddBus(context => context.GetRequiredService<InMemoryTestHarness>().BusControl);
            });

            services.AddLogging(cfg => cfg.AddDebug());
            services.AddScoped<IScopedCache, ScopedCache>();
            services.AddScoped<IConsumer<MessageA>, TestConsumer>();
            services.AddScoped(typeof(ScopedLoggerFilter<>));

            var serviceProvider = services.BuildServiceProvider();

            var harness = serviceProvider.GetRequiredService<InMemoryTestHarness>();
            var consumerHarness = harness.Consumer(() =>
            {
                return serviceProvider.GetRequiredService<IConsumer<MessageA>>();
            });

            var correlationId = Guid.NewGuid();

            await harness.Start().ConfigureAwait(false);
            try
            {
                await harness.InputQueueSendEndpoint.Send(new MessageA(), context =>
                {
                    context.CorrelationId = correlationId;
                }).ConfigureAwait(false);

                Assert.True(await harness.Consumed.Any<MessageA>());

                Assert.True(await consumerHarness.Consumed.Any<MessageA>());

                Assert.True(await harness.Published.Any<MessageB>());

                // ensure that no faults were published by the consumer
                Assert.False(await harness.Published.Any<Fault<MessageB>>());

                var publishedMessage = harness.Published.Select<MessageB>().SingleOrDefault();
                Assert.Equal(correlationId, publishedMessage.Context.CorrelationId);
            }
            finally
            {
                await harness.Stop();
            }

        }
    }
}
