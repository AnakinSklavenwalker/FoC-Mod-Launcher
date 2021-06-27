namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    public record DependencyResolverOptions
    {
        /// <summary>
        /// When set to <see langword="true"/> the complete mod dependency chain gets resolved.
        /// </summary>
        public bool ResolveCompleteChain { get; init; }

        /// <summary>
        /// When dependencies are resolved, a cycle check will be performed.
        /// When set to <see langword="true"/> this may cause an <see cref="IDependencyResolver"/> to throw an exception.
        /// </summary>
        public bool CheckForCycle { get; init; }
    }
}