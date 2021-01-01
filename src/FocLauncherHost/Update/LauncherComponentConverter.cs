using System;
using FocLauncherHost.Update.Model;
using TaskBasedUpdater;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Verification;
using Validation;

namespace FocLauncherHost.Update
{
    internal class LauncherComponentConverter : IComponentConverter<LauncherComponent>
    {
        private readonly IFullDestinationResolver _destinationResolver;

        public LauncherComponentConverter(IFullDestinationResolver destinationResolver)
        {
            Requires.NotNull(destinationResolver, nameof(destinationResolver));
            _destinationResolver = destinationResolver;
        }

        public ProductComponent Convert(LauncherComponent launcherComponent)
        {
            Requires.NotNullAllowStructs(launcherComponent, nameof(launcherComponent));
            var name = launcherComponent.Name;
            if (string.IsNullOrEmpty(name))
                throw new ComponentException("Name of a product component cannot be null or empty");
            if (launcherComponent.Destination is null)
                throw new ComponentException("Destination of a product component cannot be null.");

            var destination = _destinationResolver.GetFullDestination(launcherComponent.Destination, null);


            var newVersion = launcherComponent.GetVersion();
            var hash = launcherComponent.Sha2;
            var size = launcherComponent.Size;

            var verificationContext = VerificationContext.None;
            if (hash != null)
                verificationContext = new VerificationContext(hash, HashType.Sha256);
            var originInfo = new OriginInfo(new Uri(launcherComponent.Origin, UriKind.Absolute))
            {
                Size = size,
                VerificationContext = verificationContext
            };

            

            return new ProductComponent(name, destination)
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