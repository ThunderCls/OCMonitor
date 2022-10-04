using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OCMonitor.App.Model;
using OCMonitor.App.Service;
using Serilog;
using Serilog.Events;

namespace OCMonitor.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var customConfig = BuildConfiguration(args, hostingContext);
                    BuildLogger(customConfig);
                    config.Sources.Clear();
                    config.AddConfiguration(customConfig);
                })
                .ConfigureServices(SetupServices)
                .UseSerilog()
                .Build();

            var application = host.Services.GetService<Application>();
            if (application != null) await application.RunAsync(args);
        }

        private static void SetupServices(HostBuilderContext hostBuilderContext,
            IServiceCollection services)
        {
            services.AddSingleton<Application>();
            services.Configure<AfterBurnerSettings>(hostBuilderContext.Configuration.GetSection(nameof(AfterBurnerSettings)));
            services.Configure<MonitorSettings>(hostBuilderContext.Configuration.GetSection(nameof(MonitorSettings)));
            services.AddSingleton<IGPUService, GPUService>();
            services.AddSingleton<IAfterBurnerService, AfterBurnerService>();
            services.AddSingleton<IMonitor, Monitor>();
        }

        private static IConfiguration BuildConfiguration(string[] args,
            HostBuilderContext hostBuilderContext)
        {
            var environment = hostBuilderContext.HostingEnvironment;

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            if (environment.IsDevelopment())
            {
                config.AddUserSecrets<Program>(true);
            }

            if (args != null)
            {
                config.AddCommandLine(args);
            }

            return config.Build();
        }

        private static void BuildLogger(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                // .WriteTo.Console()
                // .WriteTo.File()
                .CreateLogger();

        }
    }
}