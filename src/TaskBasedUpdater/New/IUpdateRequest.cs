using TaskBasedUpdater.New.Product;

namespace TaskBasedUpdater.New
{
    public interface IUpdateRequest
    {
        string UpdateManifestPath { get; }
        
        UpdateRequestAction RequestedAction { get; }

        IProductReference Product { get; }
    }
}