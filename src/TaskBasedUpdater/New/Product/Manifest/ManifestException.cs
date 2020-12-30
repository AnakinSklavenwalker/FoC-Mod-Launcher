using System;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public class ManifestException : Exception
    {
        public ManifestException(string message)
            : base(message)
        {
        }

        public ManifestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}