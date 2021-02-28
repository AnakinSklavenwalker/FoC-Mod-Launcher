namespace PetroGlyph.Games.EawFoc.Mods
{
    internal static class ModDependencyUtilities
    {
        internal static bool IsRecursive(this ModDependencyResolveStrategy strategy)
        {
            return strategy == ModDependencyResolveStrategy.CreateRecursive ||
                   strategy == ModDependencyResolveStrategy.FromExistingModsRecursive;
        }

        internal static bool IsCreative(this ModDependencyResolveStrategy strategy)
        {
            return strategy == ModDependencyResolveStrategy.Create ||
                   strategy == ModDependencyResolveStrategy.CreateRecursive;
        }
    }
}