using MassTransit;
using MassTransitScopeFilter.Filters.Old;
using MassTransitScopeFilter.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.IO;
using MassTransitScopeFilter.Extensions;

namespace MassTransitScopeFilter
{
    public static class Program
    {
        private const string hostname = "[YOUR_HOSTNAME]";
        private const int port = 5672;
        private const string virtualHost = "";
        private const string user = "[YOUR_USER]";
        private const string pwd = "[YOUR_PASSWORD])";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<IApplicationService, ApplicationService>();
                    services.AddScoped<IScopedCache, ScopedCache>();

                    services.ConfigureMassTransit(hostContext.Configuration, hostname, port, virtualHost, user, pwd);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"));
                    configApp.AddJsonFile("loggingConfiguration.json", optional: false, reloadOnChange: true);
                    configApp.AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);
                })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration?.ReadFrom.Configuration(hostingContext.Configuration);
                });
    }
}
