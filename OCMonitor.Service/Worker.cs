using Microsoft.Extensions.Options;
using OCMonitor.Lib;
using OCMonitor.Service.Model;
using OCMonitor.Service.Service;

namespace OCMonitor.Service;

public class Worker : BackgroundService
{
    private enum OcStatus
    {
        None,
        Enabled,
        Disabled
    }
    
    private readonly ILogger<Worker> _logger;
    private readonly IGPUService _gpuService;
    private readonly IAfterBurnerService _afterBurnerService;
    private readonly IOptionsMonitor<MonitorSettings> _monitorSettings;
    private OcStatus _ocStatus = OcStatus.Disabled;
    
    public Worker(ILogger<Worker> logger,
        IGPUService gpuService, 
        IAfterBurnerService afterBurnerService,
        IOptionsMonitor<MonitorSettings> monitorSettings)
    {
        _logger = logger;
        _gpuService = gpuService;
        _afterBurnerService = afterBurnerService;
        _monitorSettings = monitorSettings; 
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Initiating monitoring");
        
        try
        {
            // SensorTypes: Load, Temperature, Power
            var settings = _monitorSettings.CurrentValue;
            var coreSensorThreshold = float.Parse(settings.CoreMonitor.ThresholdValue);
            var memSensorThreshold = float.Parse(settings.MemoryMonitor.ThresholdValue);
            _logger.LogInformation("CoreTrigger: {0}/{1}%", settings.CoreMonitor.SensorType.ToString(), coreSensorThreshold);
            _logger.LogInformation("MemoryTrigger: {0}/{1}%", settings.MemoryMonitor.SensorType.ToString(), memSensorThreshold);
        
            var prevCoreSensorType = settings.CoreMonitor.SensorType;
            var prevMemorySensorType = settings.MemoryMonitor.SensorType;
            var prevCoreSensorThreshold = coreSensorThreshold;
            var prevMemSensorThreshold = memSensorThreshold;
            
            // note: only support for 1 GPU
            var gpuSummary = _gpuService.GetNvidiaGpuInfo();
            var firstGpu = gpuSummary.NvidiaGpus.First();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                // SensorTypes: Load, Temperature, Power
                settings = _monitorSettings.CurrentValue;
                coreSensorThreshold = float.Parse(settings.CoreMonitor.ThresholdValue);
                memSensorThreshold = float.Parse(settings.MemoryMonitor.ThresholdValue);

                UpdateTriggerValues(coreSensorThreshold, ref prevCoreSensorThreshold, memSensorThreshold, 
                    ref prevMemSensorThreshold, settings, ref prevCoreSensorType, ref prevMemorySensorType);
                
                var coreSensor = firstGpu.Sensors.First(s => 
                    s.Identifier.ToString().Contains($"/{settings.CoreMonitor.SensorType.ToString().ToLower()}/0"));
                var memorySensor = firstGpu.Sensors.First(s => 
                    s.Identifier.ToString().Contains($"/{settings.MemoryMonitor.SensorType.ToString().ToLower()}/1"));
                if (coreSensor.Value is null && memorySensor.Value is null)
                {
                    _logger.LogWarning("Sensors values couldn't be loaded");
                    continue;
                }

                if (coreSensor.Value.HasValue && coreSensor.Value.Value >= coreSensorThreshold &&
                    memorySensor.Value.HasValue && memorySensor.Value.Value >= memSensorThreshold)
                {
                    if (_ocStatus == OcStatus.Disabled)
                    {
                        _logger.LogInformation("Thresholds reached...activating OC profile");
                        _afterBurnerService.SetOcProfile();
                        _ocStatus = OcStatus.Enabled;
                    }
                }
                else
                {
                    if (_ocStatus == OcStatus.Enabled)
                    {
                        _logger.LogInformation("Thresholds reestablished...deactivating OC profile");
                        _afterBurnerService.SetStdProfile();
                        _ocStatus = OcStatus.Disabled;
                    }
                }

                await Task.Delay(settings.IntervalSecs * 1000, stoppingToken);
                firstGpu.Update();
            }
        }
        catch (Exception e)
        {
            _logger.LogCritical(e.Message);
        }
        
        _logger.LogInformation("Monitoring terminated");
    }

    private void UpdateTriggerValues(float coreSensorThreshold, ref float prevCoreSensorThreshold, float memSensorThreshold,
        ref float prevMemSensorThreshold, MonitorSettings settings, ref SensorType prevCoreSensorType,
        ref SensorType prevMemorySensorType)
    {
        if ((int) coreSensorThreshold == (int) prevCoreSensorThreshold &&
            (int) memSensorThreshold == (int) prevMemSensorThreshold &&
            settings.CoreMonitor.SensorType == prevCoreSensorType &&
            settings.MemoryMonitor.SensorType == prevMemorySensorType) return;
        
        prevCoreSensorType = settings.CoreMonitor.SensorType;
        prevMemorySensorType = settings.MemoryMonitor.SensorType;
        prevCoreSensorThreshold = coreSensorThreshold;
        prevMemSensorThreshold = memSensorThreshold;

        _logger.LogInformation("CoreTrigger: {0}/{1}%",
            settings.CoreMonitor.SensorType.ToString(), coreSensorThreshold);
        _logger.LogInformation("MemoryTrigger: {0}/{1}%",
            settings.MemoryMonitor.SensorType.ToString(), memSensorThreshold);
    }
}