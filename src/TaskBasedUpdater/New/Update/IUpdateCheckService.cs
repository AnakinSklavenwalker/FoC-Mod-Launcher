using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedUpdater.New.Update
{
    public interface IUpdateCheckService
    {
        bool IsCheckingForUpdates { get; }

        Task<UpdateCheckResult> CheckForUpdates(UpdateRequest updateRequest, CancellationToken token = default);
    }
}