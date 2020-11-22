using System;
using System.Collections.Generic;
using System.Linq;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.UpdateItem;
using Validation;

namespace TaskBasedUpdater.New.Update
{
    public class UpdateCatalogBuilder : IUpdateCatalogBuilder
    {
        public IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IAvailableProductCatalog availableCatalog,
            UpdateRequestAction action)
        {
            Requires.NotNull(installedCatalog, nameof(installedCatalog));
            Requires.NotNull(availableCatalog, nameof(availableCatalog));

            if (!ProductReferenceEqualityComparer.Default.Equals(installedCatalog.Product, availableCatalog.Product))
                throw new InvalidOperationException("Cannot build update catalog from different products.");

            var currentItems = installedCatalog.Items.ToHashSet(UpdateItemIdentityComparer.Default);
            var availableItems = installedCatalog.Items.ToHashSet(UpdateItemIdentityComparer.Default);

            if (!currentItems.Any() && !availableItems.Any())
                return new UpdateCatalog(availableCatalog.Product, new IUpdateItem[0], action);

            if (!availableItems.Any())
                return ShallowCatalogWithAction(availableCatalog.Product, installedCatalog, UpdateAction.Delete, action);

            if (!currentItems.Any())
                return ShallowCatalogWithAction(availableCatalog.Product, installedCatalog, UpdateAction.Update, action);

            var catalogItems = new List<IUpdateItem>();
            foreach (var availableItem in availableItems)
            {
                // TODO: Implement a real check
                if (!currentItems.TryGetValue(availableItem, out var current))
                {
                    //availableItem.RequiredAction = UpdateAction.Update;
                    catalogItems.Add(availableItem);
                }
                else
                {
                    currentItems.Remove(current);
                    if (!action.HasFlag(UpdateRequestAction.Repair))
                    {
                        // availableItem.RequiredAction = UpdateAction.Keep;
                        catalogItems.Add(availableItem);
                    }
                    else
                    {
                        // TODO: Check whether to keep or update
                        var item = Compare(current, availableItem);
                        catalogItems.Add(item);
                    }
                }
            }

            foreach (var currentItem in currentItems)
            {
                //updateItem.RequiredAction = UpdateAction.Delete;
                catalogItems.Add(currentItem);
            }

            return new UpdateCatalog(availableCatalog.Product, catalogItems, action);
        }

        private static IUpdateItem Compare(IUpdateItem current, IUpdateItem available)
        {
            return current;
        }

        private static IUpdateCatalog ShallowCatalogWithAction(IProductReference product, ICatalog catalog,
            UpdateAction updateAction, UpdateRequestAction requestAction)
        {
            return new UpdateCatalog(product,
                catalog.Items.Select(x =>
                {
                    var copy = new UpdateItem.UpdateItem(x) {RequiredAction = updateAction};
                    return copy;
                }), requestAction);

        }
    }
}