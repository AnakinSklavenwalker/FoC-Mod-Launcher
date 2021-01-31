using System.Threading;
using System.Threading.Tasks;

namespace ProductUpdater.New.Update.Service
{
    public interface IUpdateCheckService
    {
        bool IsCheckingForUpdates { get; }

        Task<UpdateCheckResult> CheckForUpdates(UpdateRequest updateRequest, CancellationToken token = default);
    }
}