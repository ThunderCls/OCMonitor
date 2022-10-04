using System.Diagnostics;
using Microsoft.Extensions.Options;
using OCMonitor.Service.Model;

namespace OCMonitor.Service.Service;

public class AfterBurnerService : IAfterBurnerService
{
    private readonly AfterBurnerSettings _settings;
    
    public AfterBurnerService(IOptionsMonitor<AfterBurnerSettings> monitorSettings)
    {
        _settings = monitorSettings.CurrentValue;
    }

    public void SetOcProfile()
    {
        SetProfile(_settings.OcProfile);
    }

    public void SetStdProfile()
    {
        SetProfile(_settings.StdProfile);
    }
    
    private void SetProfile(string profile)
    {
        Process.Start("cmd.exe", @$"/k ""{_settings.ExecutablePath}"" \s {profile}");
    }
}