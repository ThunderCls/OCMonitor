using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OCMonitor.App;

public class Application
{
    private readonly ILogger<Application> _logger;
    private readonly IMonitor _monitor;

    public Application(ILogger<Application> logger,
        IMonitor monitor)
    {
        _logger = logger;
        _monitor = monitor;
    }

    public async Task RunAsync(string[] args)
    {
        // Utils.HideCurrentConsoleWindow();
        Console.CancelKeyPress += OnShutDown;
        _logger.LogInformation("Initiating monitoring");
        
        await Task.Run(() =>
        {
            try
            {
                _monitor.Start();
                _monitor.CancellationTokenSource.Token.WaitHandle.WaitOne();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception.Message);
            }
        });
    }

    private void OnShutDown(object sender, EventArgs eventArgs)
    {
        _logger.LogInformation("Terminating monitoring");
        _monitor.Stop();
    }
}
