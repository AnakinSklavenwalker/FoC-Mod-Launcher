using System;
using FocLauncher.UpdateMetadata;
using TaskBasedUpdater.Component;

namespace FocLauncherHost.Utilities
{
    public static class CatalogExtensions
    {
        public static ProductComponent? DependencyToComponent(Dependency dependency)
        {
            if (string.IsNullOrEmpty(dependency.Name) || string.IsNullOrEmpty(dependency.Destination))
                return null;

            if (string.IsNullOrEmpty(dependency.Origin))
                return new ProductComponent(dependency.Name, GetRealDependencyDestination(dependency));
            
            var newVersion = dependency.GetVersion();
            var hash = dependency.Sha2;
            var size = dependency.Size;

            ValidationContext? validationContext = null;
            if (hash != null)
                validationContext = new ValidationContext { Hash = hash, HashType = HashType.Sha256 };
            var originInfo = new OriginInfo(new Uri(dependency.Origin, UriKind.Absolute), newVersion, size, validationContext);

            return new ProductComponent(dependency.Name, GetRealDependencyDestination(dependency))
            {
                OriginInfo = originInfo
            };
        }

        private static string GetRealDependencyDestination(Dependency dependency)
        {
            var destination = Environment.ExpandEnvironmentVariables(dependency.Destination);
            if (!Uri.TryCreate(destination, UriKind.Absolute, out var uri))
                throw new InvalidOperationException($"No absolute dependency destination: {destination}");
            return uri.LocalPath;
        }
    }
}