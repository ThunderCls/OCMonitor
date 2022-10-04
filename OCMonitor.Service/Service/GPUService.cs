using OCMonitor.Lib;
using OCMonitor.Lib.Nvidia;
using OCMonitor.Service.Model;

namespace OCMonitor.Service.Service;

public class GPUService : IGPUService
{
    public GPUSummary GetNvidiaGpuInfo()
    {
        var summary = new GPUSummary();
        
        if (NVAPI.NvAPI_GetInterfaceVersionString(out var version) == NvStatus.OK)
            summary.NvApiVersion = version;
        
        var handles = new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
        if (NVAPI.NvAPI_EnumPhysicalGPUs == null) 
            throw new ApplicationException("Error: NvAPI_EnumPhysicalGPUs not available");
        
        var enumStatus = NVAPI.NvAPI_EnumPhysicalGPUs(handles, out var gpuCount);
        if (enumStatus != NvStatus.OK)
            throw new ApplicationException($"Error: {enumStatus} when enumerating gpus");
        
        summary.GpuCount = gpuCount;

        var result = NVML.NvmlInit();
        if (result != NVML.NvmlReturn.Success)
            throw new ApplicationException($"Error: {result} when initiating NVML");
        
        var displayHandles = new Dictionary<NvPhysicalGpuHandle, NvDisplayHandle>();
        if (NVAPI.NvAPI_EnumNvidiaDisplayHandle != null &&
            NVAPI.NvAPI_GetPhysicalGPUsFromDisplay != null) 
        {
            var status = NvStatus.OK;
            var i = 0;
            while (status == NvStatus.OK) 
            {
                var displayHandle = new NvDisplayHandle();
                status = NVAPI.NvAPI_EnumNvidiaDisplayHandle(i, ref displayHandle);
                i++;

                if (status != NvStatus.OK) continue;
                
                var handlesFromDisplay = new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
                if (NVAPI.NvAPI_GetPhysicalGPUsFromDisplay(displayHandle, 
                        handlesFromDisplay, out var countFromDisplay) != NvStatus.OK) continue;
                
                for (var j = 0; j < countFromDisplay; j++) 
                {
                    if (!displayHandles.ContainsKey(handlesFromDisplay[j]))
                        displayHandles.Add(handlesFromDisplay[j], displayHandle);
                }
            }
        }

        for (var i = 0; i < gpuCount; i++) 
        {
            displayHandles.TryGetValue(handles[i], out var displayHandle);
            summary.NvidiaGpus.Add(new NvidiaGPU(i, handles[i], displayHandle, new Settings()));
        }

        return summary;
    }
}