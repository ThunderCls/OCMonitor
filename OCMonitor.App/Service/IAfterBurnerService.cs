using System.Threading.Tasks;

namespace OCMonitor.App.Service;

public interface IAfterBurnerService
{
    void SetOcProfile();
    void SetStdProfile();
}