using OCMonitor.App.Model;

namespace OCMonitor.App.Service;

public interface IGPUService
{
    GPUSummary GetNvidiaGpuInfo();
}