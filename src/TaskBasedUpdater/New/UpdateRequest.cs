using TaskBasedUpdater.New.Product;

namespace TaskBasedUpdater.New
{
    public sealed record UpdateRequest : IUpdateRequest
    {
        public string UpdateManifestPath { get; init; }
        public UpdateRequestAction RequestedAction { get; init; }
        public IProductReference Product { get; init; }
    }
}