﻿using System;
using System.Collections.Generic;
using System.Linq;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New.Product;
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

            var currentItems = installedCatalog.Items.ToHashSet(ProductComponentIdentityComparer.VersionIndependent);
            var availableItems = availableCatalog.Items.ToHashSet(ProductComponentIdentityComparer.VersionIndependent);

            if (!currentItems.Any() && !availableItems.Any())
                return new UpdateCatalog(availableCatalog.Product, new ProductComponent[0], action);

            if (!availableItems.Any())
                return ShallowCatalogWithAction(availableCatalog.Product, installedCatalog, ComponentAction.Delete, action);

            if (!currentItems.Any())
                return ShallowCatalogWithAction(availableCatalog.Product, installedCatalog, ComponentAction.Update, action);

            var catalogItems = new List<ProductComponent>();
            foreach (var availableItem in availableItems)
            {
                ProductComponent component = null;
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
                        //updateItem = availableItem with{ RequiredAction = UpdateAction.Keep};
                        catalogItems.Add(component);
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

        private static ProductComponent Compare(ProductComponent current, ProductComponent available)
        {
            return current;
        }

        private static IUpdateCatalog ShallowCatalogWithAction(IProductReference product, ICatalog catalog,
            ComponentAction updateAction, UpdateRequestAction requestAction)
        {
            return null;
            //return new UpdateCatalog(product,
            //    catalog.Items.Select(x =>
            //    {
            //        var copy = new UpdateItem.UpdateItem(x) {RequiredAction = updateAction};
            //        return copy;
            //    }), requestAction);

        }
    }
}