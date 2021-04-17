namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    public record DependencyResolverOptions
    {
        public bool Recursive { get; init; }

        public bool Traverse { get; init; }

        public ModDependencyTraverser.TraverseDirection TraverseDirection { get; init; }
    }
}