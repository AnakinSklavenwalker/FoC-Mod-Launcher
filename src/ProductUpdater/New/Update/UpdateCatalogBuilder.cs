using System;
using System.Collections.Generic;
using System.Linq;
using ProductMetadata;
using ProductMetadata.Component;
using ProductMetadata.Manifest;
using Validation;

namespace ProductUpdater.New.Update
{
    public class UpdateCatalogBuilder : IUpdateCatalogBuilder
    {
        public IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IAvailableProductManifest availableCatalog)
        {
            return Build(installedCatalog, availableCatalog, GetComponentAction);
        }

        public IUpdateCatalog Build(IInstalledProductCatalog installedCatalog,
            IAvailableProductManifest availableCatalog, CompareComponentDelegate compareComponents)
        {
            Requires.NotNull(installedCatalog, nameof(installedCatalog));
            Requires.NotNull(availableCatalog, nameof(availableCatalog));

            if (!ProductReferenceEqualityComparer.Default.Equals(installedCatalog.Product.ProductReference, availableCatalog.Product))
                throw new InvalidOperationException("Cannot build update catalog from different products.");

            var currentItems = installedCatalog.Items.ToHashSet(ProductComponentIdentityComparer.VersionIndependent);
            var availableItems = availableCatalog.Items.ToHashSet(ProductComponentIdentityComparer.VersionIndependent);

            if (!currentItems.Any() && !availableItems.Any())
                return new UpdateCatalog(availableCatalog.Product, new ProductComponent[0]);

            if (!availableItems.Any())
                return ShallowCatalogWithAction(availableCatalog.Product, installedCatalog, ComponentAction.Delete, component => component.DetectedState);

            if (!currentItems.Any())
                return ShallowCatalogWithAction(availableCatalog.Product, availableCatalog, ComponentAction.Update, _ => DetectionState.Absent);

            var catalogItems = new List<ProductComponent>();
            foreach (var availableItem in availableItems)
            {
                if (availableItem.OriginInfo is null)
                    throw new ComponentException("Update Catalog Component must have origin data information.");

                if (!currentItems.TryGetValue(availableItem, out var current))
                {
                    ProductComponent component = availableItem with
                    {
                        RequiredAction = ComponentAction.Update, 
                        DetectedState = DetectionState.Absent
                    };
                    catalogItems.Add(component);
                }
                else
                {
                    currentItems.Remove(current);
                    var componentAction = compareComponents(current, availableItem);
                    var component = availableItem with
                    {
                        RequiredAction = componentAction, 
                        DetectedState = DetectionState.Present
                    };
                    catalogItems.Add(component);
                }
            }

            // Remove deprecated components
            foreach (var currentItem in currentItems)
            {
                var component = currentItem with
                {
                    RequiredAction = ComponentAction.Delete, 
                    DetectedState = DetectionState.Present
                };
                catalogItems.Add(component);
            }

            return new UpdateCatalog(availableCatalog.Product, catalogItems);
        }
        
        public virtual ComponentAction GetComponentAction(ProductComponent current, ProductComponent available)
        {
            if (!ProductComponentIdentityComparer.VersionIndependent.Equals(current, available))
                throw new InvalidOperationException(
                    $"Cannot get action from not-matching product components {current.Name}:{available.Name}");
            if (available.OriginInfo is null)
                throw new ComponentException("Update Catalog Component must have origin data information.");

            if (current.DetectedState != DetectionState.Present)
                return ComponentAction.Update;

            if (available.CurrentVersion is null && available.OriginInfo is null && available.DiskSize is null)
                return ComponentAction.Keep;

            if (available.CurrentVersion is not null && 
                !ProductComponentIdentityComparer.Default.Equals(current, available))
                return ComponentAction.Update;

            if (available.OriginInfo!.Size is not null && current.DiskSize is not null &&
                !current.DiskSize.Value.Equals(available.OriginInfo.Size.Value))
                return ComponentAction.Update;

            if (!CompareVerificationContext(current.IntegrityInformation, available.OriginInfo.IntegrityInformation))
                return ComponentAction.Update;
            
            return ComponentAction.Keep;
        }

        private static bool CompareVerificationContext(ComponentIntegrityInformation current, ComponentIntegrityInformation available)
        {
            return available.Equals(ComponentIntegrityInformation.None) ||
                   new IntegrityInformationComparer().Equals(current, available);
        }

        private static IUpdateCatalog ShallowCatalogWithAction(IProductReference product, 
            ICatalog catalog, ComponentAction updateAction, Func<ProductComponent,DetectionState> detectionStateFunc)
        {
            return new UpdateCatalog(product,
                catalog.Items.Select(c => c with
                {
                    RequiredAction = updateAction, 
                    DetectedState = detectionStateFunc(c)
                }));
        }
    }
}