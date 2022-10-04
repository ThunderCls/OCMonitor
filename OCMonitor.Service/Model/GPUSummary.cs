using OCMonitor.Lib.Nvidia;

namespace OCMonitor.Service.Model;

public class GPUSummary
{
    public string NvApiVersion { get; set; }
    public int GpuCount { get; set; }
    public List<NvidiaGPU> NvidiaGpus { get; set; } = new();
}