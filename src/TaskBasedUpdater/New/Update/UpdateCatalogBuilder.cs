using System;
using System.Collections.Generic;
using System.Linq;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Product.Manifest;
using TaskBasedUpdater.Verification;
using Validation;

namespace TaskBasedUpdater.New.Update
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
                return ShallowCatalogWithAction(availableCatalog.Product, installedCatalog, ComponentAction.Delete);

            if (!currentItems.Any())
                return ShallowCatalogWithAction(availableCatalog.Product, availableCatalog, ComponentAction.Update);

            var catalogItems = new List<ProductComponent>();
            foreach (var availableItem in availableItems)
            {
                if (availableItem.OriginInfo is null)
                    throw new ComponentException("Update Catalog Component must have origin data information.");

                if (!currentItems.TryGetValue(availableItem, out var current))
                {
                    ProductComponent component = availableItem with { RequiredAction = ComponentAction.Update };
                    catalogItems.Add(component);
                }
                else
                {
                    currentItems.Remove(current);
                    var componentAction = compareComponents(current, availableItem);
                    var component = availableItem with { RequiredAction = componentAction };
                    catalogItems.Add(component);
                }
            }

            // Remove deprecated components
            foreach (var currentItem in currentItems)
            {
                var component = currentItem with { RequiredAction = ComponentAction.Delete };
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

            if (current.CurrentState != CurrentState.Installed)
                return ComponentAction.Update;

            if (available.CurrentVersion is null && available.OriginInfo is null && available.DiskSize is null)
                return ComponentAction.Keep;

            if (available.CurrentVersion is not null && 
                !ProductComponentIdentityComparer.Default.Equals(current, available))
                return ComponentAction.Update;

            if (available.OriginInfo!.Size is not null && current.DiskSize is not null &&
                !current.DiskSize.Value.Equals(available.OriginInfo.Size.Value))
                return ComponentAction.Update;

            if (!CompareVerificationContext(current.VerificationContext, available.OriginInfo.VerificationContext))
                return ComponentAction.Update;
            
            return ComponentAction.Keep;
        }

        private static bool CompareVerificationContext(VerificationContext current, VerificationContext available)
        {
            return available.Equals(VerificationContext.None) || new VerificationContextComparer().Equals(current, available);
        }

        private static IUpdateCatalog ShallowCatalogWithAction(IProductReference product, 
            ICatalog catalog, ComponentAction updateAction)
        {
            return new UpdateCatalog(product,
                catalog.Items.Select(c => c with { RequiredAction = updateAction}));
        }

        private class VerificationContextComparer : IEqualityComparer<VerificationContext>
        {
            public bool Equals(VerificationContext x, VerificationContext y)
            {
                return x.HashType == y.HashType && x.Hash.SequenceEqual(y.Hash);
            }

            public int GetHashCode(VerificationContext obj)
            {
                unchecked
                {
                    return (obj.Hash.GetHashCode() * 397) ^ (int)obj.HashType;
                }
            }
        }
    }
}