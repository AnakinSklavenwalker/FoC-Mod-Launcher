using System;
using CommonUtilities;
using FocLauncherHost.Update.Model;
using ProductMetadata.Component;
using Validation;

namespace FocLauncherHost.Update
{
    //internal class LauncherComponentConverter : IComponentConverter<LauncherComponent>
    //{
    //    private readonly IFullDestinationResolver _destinationResolver;

    //    public LauncherComponentConverter(IFullDestinationResolver destinationResolver)
    //    {
    //        Requires.NotNull(destinationResolver, nameof(destinationResolver));
    //        _destinationResolver = destinationResolver;
    //    }

    //    public ProductComponent Convert(LauncherComponent launcherComponent)
    //    {
    //        Requires.NotNullAllowStructs(launcherComponent, nameof(launcherComponent));
    //        var name = launcherComponent.Name;
    //        if (string.IsNullOrEmpty(name))
    //            throw new ComponentException("Name of a product component cannot be null or empty");
    //        if (launcherComponent.Destination is null)
    //            throw new ComponentException("Destination of a product component cannot be null.");

    //        var destination = _destinationResolver.GetFullDestination(launcherComponent.Destination, null);


    //        var newVersion = launcherComponent.GetVersion();
    //        var hash = launcherComponent.Sha2;
    //        var size = launcherComponent.Size;

    //        var integrityInformation = ComponentIntegrityInformation.None;
    //        if (hash != null)
    //            integrityInformation = new ComponentIntegrityInformation(hash, HashType.Sha256);
    //        var originInfo = new OriginInfo(new Uri(launcherComponent.Origin, UriKind.Absolute))
    //        {
    //            Size = size,
    //            IntegrityInformation = integrityInformation
    //        };

            

    //        return new ProductComponent(name, destination)
    //        {
    //            OriginInfo = originInfo,
    //            CurrentVersion = newVersion
    //        };
    //    }
    //}
}