using System.Collections.Generic;
using OCMonitor.Lib;

namespace OCMonitor.App.Model;

public class MonitorSettings
{
    public Trigger CoreMonitor { get; set; }
    public Trigger MemoryMonitor { get; set; }
    public int IntervalSecs { get; set; }
}

public class Trigger
{
    public SensorType SensorType { get; set; }
    public string ThresholdValue { get; set; }
}