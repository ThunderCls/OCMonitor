using OCMonitor.Service.Model;

namespace OCMonitor.Service.Service;

public interface IGPUService
{
    GPUSummary GetNvidiaGpuInfo();
}