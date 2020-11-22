using TaskBasedUpdater.New.Product;

namespace TaskBasedUpdater.New.Update
{
    public sealed record UpdateRequest : IUpdateRequest
    {
        public string UpdateManifestPath { get; init; }
        public UpdateRequestAction RequestedAction { get; init; }
        public IProductReference Product { get; init; }
    }
}