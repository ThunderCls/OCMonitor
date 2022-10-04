using System.Threading;
using System.Threading.Tasks;

namespace OCMonitor.App;

public interface IMonitor
{
    Task Start();
    WaitHandle Stop();
    CancellationTokenSource CancellationTokenSource { get; }
}