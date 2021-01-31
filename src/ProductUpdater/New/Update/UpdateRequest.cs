using ProductMetadata.Manifest;
using Validation;

namespace ProductUpdater.New.Update
{
    public class UpdateRequest
    {
        public ProductManifestLocation ManifestLocation { get; }

        public UpdateRequest(ProductManifestLocation manifestLocation)
        {
            Requires.NotNull(manifestLocation, nameof(manifestLocation));
            ManifestLocation = manifestLocation;
        }
    }
}
