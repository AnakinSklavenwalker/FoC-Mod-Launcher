using System;
using System.Globalization;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Name
{
    public class DirectoryModNameResolver : ModNameResolverBase
    {
        public DirectoryModNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override string ResolveCore(IModReference modReference, CultureInfo culture)
        {
            if (modReference.Type == ModType.Virtual)
                throw new ModException("Cannot resolve name for virtual mods.");
            if (modReference is IPhysicalMod mod)
                return BeautifyDirectoryName(mod.Directory.Name);
            var fs = ServiceProvider.GetService<IFileSystem>() ?? new FileSystem();
            var directoryName = fs.Path.GetDirectoryName(modReference.Identifier);
            return BeautifyDirectoryName(directoryName);
        }

        private static string BeautifyDirectoryName(string directoryName)
        {
            var removedUnderscore = directoryName.Replace('_', ' ');
            return removedUnderscore;
        }
    }
}