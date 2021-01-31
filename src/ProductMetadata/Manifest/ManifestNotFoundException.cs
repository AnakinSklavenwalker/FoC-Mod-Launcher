namespace ProductMetadata.Manifest
{
    public class ManifestNotFoundException : ManifestException
    {
        public ManifestNotFoundException(string message) : base(message)
        {
        }
    }
}