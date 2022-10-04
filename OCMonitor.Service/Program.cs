using OCMonitor.Service;
using OCMonitor.Service.Model;
using OCMonitor.Service.Service;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureAppConfiguration((_, config) =>
    {
        BuildLogger(config.Build());
    })
    .ConfigureServices(SetupServices)
    .UseSerilog()
    .Build();

try
{
    Log.Information("Starting up the service");
    await host.RunAsync();
}
catch (Exception exception)
{
    Log.Error("Error while starting the service");
    Log.Error(exception.Message);
}
finally
{
    Log.Information("Service terminated");
    Log.CloseAndFlush();
}

static void SetupServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
{
    services.AddHostedService<Worker>();
    services.Configure<AfterBurnerSettings>(hostBuilderContext.Configuration.GetSection(nameof(AfterBurnerSettings)));
    services.Configure<MonitorSettings>(hostBuilderContext.Configuration.GetSection(nameof(MonitorSettings)));
    services.AddSingleton<IGPUService, GPUService>();
    services.AddSingleton<IAfterBurnerService, AfterBurnerService>();
}

static void BuildLogger(IConfiguration configuration)
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext()
        .CreateLogger();
}