using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Validation;

namespace TaskBasedUpdater.Component
{
    public static class ComponentFileFactory
    {
        public static ProductComponent FromFile(IFileInfo file, Func<IFileInfo, Version?>? versionGetter, HashType hashType)
        {
            Requires.NotNull(file, nameof(file));
            if (file.IsDirectory)
                throw new ComponentException("Unable to create a ProductComponent from a directory.");
            var name = file.Name;
            var destination = Path.GetDirectoryName(file.PhysicalPath);
            var component = new ProductComponent(name, destination!);
            return FromFile(component, file, versionGetter, hashType);
        }

        public static ProductComponent FromFile(ProductComponent baseComponent,
            IFileInfo file, Func<IFileInfo, Version?>? versionGetter, HashType hashType)
        {
            if (file.IsDirectory)
                throw new ComponentException("Unable to create a ProductComponent from a directory.");
            if (!file.Exists)
                return baseComponent with { CurrentState = CurrentState.Removed };

            Version? version = null;
            if (versionGetter is not null)
                version = versionGetter(file);

            ValidationContext? validationContext = null;
            if (hashType != HashType.None)
            {
            }

            return baseComponent with
                {
                CurrentState = CurrentState.Installed,
                CurrentVersion = version,
                ValidationContext = validationContext
                };
        }
    }
}