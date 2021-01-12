using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.New.Product.Component;
using Validation;

namespace TaskBasedUpdater.Validation
{
    internal class DiskSpaceCalculator
    {
        public IDictionary<string, DriveSpaceData> CalculatedDiskSizes { get; }

        public bool HasEnoughDiskSpace { get; } = true;

        internal DiskSpaceCalculator(
            IServiceProvider serviceProvider,
            ProductComponent productComponent, 
            long additionalBuffer = 0, 
            CalculationOption option = CalculationOption.All)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            
            CalculatedDiskSizes = new Dictionary<string, DriveSpaceData>(StringComparer.OrdinalIgnoreCase);
            var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
            var destinationRoot = fileSystem.Path.GetPathRoot(productComponent.GetFilePath());

            // TODO: split-projects
            var backupRoot = string.Empty;
            //var backupRoot = fileSystem.Path.GetPathRoot(UpdateConfiguration.Instance.BackupPath);

            if (string.IsNullOrEmpty(backupRoot)) 
                backupRoot = destinationRoot;

            

            if (option.HasFlag(CalculationOption.Download) && UpdateItemDownloadPathStorage.Instance.TryGetValue(productComponent, out var downloadPath))
            {
                var downloadRoot = fileSystem.Path.GetPathRoot(downloadPath);
                if (!string.IsNullOrEmpty(downloadPath))
                    SetSizeMembers(productComponent.OriginInfo?.Size, downloadRoot!);
            }

            if (option.HasFlag(CalculationOption.Install))
                SetSizeMembers(productComponent.OriginInfo?.Size, destinationRoot!);
            if (option.HasFlag(CalculationOption.Backup))
                SetSizeMembers(productComponent.DiskSize, backupRoot!);

            foreach (var sizes in CalculatedDiskSizes)
            {
                try
                {
                    var driveFreeSpace = fileSystem.GetDriveFreeSpace(sizes.Key);
                    sizes.Value.AvailableDiskSpace = driveFreeSpace;
                    sizes.Value.HasEnoughDiskSpace = driveFreeSpace >= sizes.Value.RequestedSize + additionalBuffer;
                }
                catch
                {
                    sizes.Value.HasEnoughDiskSpace = false;
                    HasEnoughDiskSpace = false;
                }
            }
        }
        
        private void SetSizeMembers(long? actualSize, string drive)
        {
            if (!actualSize.HasValue)
                return;
            if (!CalculatedDiskSizes.ContainsKey(drive))
                CalculatedDiskSizes.Add(drive, new DriveSpaceData(actualSize.Value, drive));
            else
            {
                CalculatedDiskSizes[drive].RequestedSize += actualSize.Value;
            }
        }

        [Flags]
        public enum CalculationOption
        {
            Install = 1,
            Download = 2,
            Backup = 4,
            All = Install | Download | Backup
        }
    }
}