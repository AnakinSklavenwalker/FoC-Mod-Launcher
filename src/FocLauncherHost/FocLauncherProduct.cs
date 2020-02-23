﻿using FocLauncher;
using FocLauncherHost.Updater;

namespace FocLauncherHost
{
    internal class FocLauncherProduct : IProductInfo
    {
        private static FocLauncherProduct _instance;

        public string Name { get; }
        public string Author => LauncherConstants.Author;
        public string AppDataPath => LauncherConstants.ApplicationBasePath;
        public string CurrentLocation => GetType().Assembly.Location;

        public PreviewType PreviewType { get; private set; }

        // TODO: Decide how to get data
        public bool IsPreviewInstance { get; } = false;

        public static FocLauncherProduct Instance => _instance ??= new FocLauncherProduct();

        private FocLauncherProduct()
        {
            Name = GetProductName();
        }

        private string GetProductName()
        {
            var name = LauncherConstants.ProductName;
            if (IsPreviewInstance && PreviewType != PreviewType.None)
                name = $"{name}-{PreviewType}";
            return name;
        }
    }

    internal enum PreviewType
    {
        None,
        Beta,
        Alpha
    }
}