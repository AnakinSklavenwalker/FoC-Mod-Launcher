using System;
using System.Threading;
using TaskBasedUpdater.Configuration;

namespace TaskBasedUpdater.New
{
    public interface IUpdateManager : IDisposable
    {
        IUpdateConfiguration UpdateConfiguration { get; }

        void Initialize();

        IUpdateResultInformation Update(IUpdateCatalog updateCatalog, CancellationToken cancellation);
    }
}