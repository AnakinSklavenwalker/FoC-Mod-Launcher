using System;
using FocLauncherHost.Update.Model;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Verification;
using Validation;

namespace FocLauncherHost.Update
{
    internal class LauncherComponentConverter : IComponentConverter<LauncherComponent>
    {
        public ProductComponent Convert(LauncherComponent dependency)
        {
            Requires.NotNullAllowStructs(dependency, nameof(dependency));
            return null;
        }

        public static ProductComponent? DependencyToComponent(LauncherComponent dependency)
        {
            if (string.IsNullOrEmpty(dependency.Name) || string.IsNullOrEmpty(dependency.Destination))
                return null;

            if (string.IsNullOrEmpty(dependency.Origin))
                return new ProductComponent(dependency.Name, GetRealDependencyDestination(dependency));
            
            var newVersion = dependency.GetVersion();
            var hash = dependency.Sha2;
            var size = dependency.Size;

            var verificationContext = VerificationContext.None;
            if (hash != null)
                verificationContext = new VerificationContext(hash, HashType.Sha256);
            var originInfo = new OriginInfo(new Uri(dependency.Origin, UriKind.Absolute))
            {
                Size = size,
                VerificationContext = verificationContext
            };

            return new ProductComponent(dependency.Name, GetRealDependencyDestination(dependency))
            {
                OriginInfo = originInfo,
                CurrentVersion = newVersion
            };
        }

        private static string GetRealDependencyDestination(LauncherComponent dependency)
        {
            var destination = Environment.ExpandEnvironmentVariables(dependency.Destination);
            if (!Uri.TryCreate(destination, UriKind.Absolute, out var uri))
                throw new InvalidOperationException($"No absolute dependency destination: {destination}");
            return uri.LocalPath;
        }
    }
}