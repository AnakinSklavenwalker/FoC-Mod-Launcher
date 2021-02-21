using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FocLauncher;
using FocLauncherHost.Update.Model;
using ProductMetadata;
using ProductMetadata.Manifest;
using Requires = Validation.Requires;

namespace FocLauncherHost.Update
{
    //internal class LauncherManifestFinder : ILauncherUpdateManifestFinder
    //{
    //    private readonly LauncherUpdateSearchSettings _updateSearchSettings;
    //    private readonly FallbackSearchAction? _fallbackSearchAction;
        
    //    public LauncherManifestFinder(LauncherUpdateSearchSettings updateSearchSettings) 
    //        : this(updateSearchSettings, DefaultFallbackSearchAction)
    //    {
    //    }

    //    internal LauncherManifestFinder(LauncherUpdateSearchSettings updateSearchSettings,
    //        FallbackSearchAction? fallbackSearchAction)
    //    {
    //        Requires.NotNull(updateSearchSettings, nameof(updateSearchSettings));
    //        _updateSearchSettings = updateSearchSettings;
    //        _fallbackSearchAction = fallbackSearchAction;
    //    }

    //    public LauncherUpdateManifestModel FindMatching(LauncherUpdateManifestContainer container, ManifestLocation manifestLocation)
    //    {
    //        Requires.NotNull(container, nameof(container));
    //        Requires.NotNull(manifestLocation, nameof(manifestLocation));
            
    //        var matchingProductsByName = container.Manifests.Where(x =>
    //            x.Name.Equals(manifestLocation.Product.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();

    //        if (!matchingProductsByName.Any())
    //            throw new ManifestNotFoundException($"Unable to find matching manifest from {container} for product {manifestLocation.Product}");

    //        var applicationType = ConvertReleaseType(manifestLocation.Product.ReleaseType);

    //        var matchingManifest = matchingProductsByName.FirstOrDefault(x => x.ApplicationType == applicationType);
    //        if (matchingManifest != null)
    //            return matchingManifest;

    //        if (_fallbackSearchAction is null || _updateSearchSettings.UpdateMode == UpdateMode.Explicit ||
    //            _updateSearchSettings.UpdateMode == UpdateMode.NoFallback)
    //            throw new ManifestNotFoundException(
    //                $"Unable to find matching manifest from {container} for product {manifestLocation.Product} with settings: {_updateSearchSettings}");

    //        var useFallback = true;
    //        if (_updateSearchSettings.UpdateMode == UpdateMode.AskFallbackStable)
    //        {
    //            // TODO: Implement message system
    //            var messageBoxResult = MessageBox.Show(
    //                $"No updates for {applicationType}-Version available. Do you want to update to the latest stable version instead?",
    //                "FoC Launcher", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
    //            useFallback = messageBoxResult == MessageBoxResult.Yes;
    //        }

    //        if (!useFallback)
    //            throw new ManifestNotFoundException(
    //                $"Unable to find matching manifest from {container} for product {manifestLocation.Product} with settings: {_updateSearchSettings}");

    //        var manifest = _fallbackSearchAction(matchingProductsByName);
    //        if (manifest is null)
    //            throw new ManifestNotFoundException(
    //                $"Unable to find matching manifest from {container} for product {manifestLocation.Product} using fallback option.");

    //        return manifest;
    //    }

    //    private static ApplicationType ConvertReleaseType(ProductReleaseType releaseType)
    //    {
    //        switch (releaseType)
    //        {
    //            case ProductReleaseType.Rtm:
    //            case ProductReleaseType.Stable:
    //                return ApplicationType.Stable;
    //            case ProductReleaseType.PreAlpha:
    //            case ProductReleaseType.Alpha:
    //                return ApplicationType.Test;
    //            case ProductReleaseType.Beta:
    //            case ProductReleaseType.ReleaseCandidate:
    //                return ApplicationType.Beta;
    //            default:
    //                throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null);
    //        }
    //    }


    //    private static LauncherUpdateManifestModel? DefaultFallbackSearchAction(IEnumerable<LauncherUpdateManifestModel> manifests)
    //    {
    //        return manifests.FirstOrDefault(x => x.ApplicationType == ApplicationType.Stable);
    //    }
    //}
}