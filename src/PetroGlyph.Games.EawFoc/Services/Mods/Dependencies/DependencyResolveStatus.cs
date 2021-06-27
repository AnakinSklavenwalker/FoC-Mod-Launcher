namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    public enum DependencyResolveStatus
    {
        /// <summary>
        /// Dependencies are not yet resolved.
        /// </summary>
        None,
        /// <summary>
        /// Dependency are currently resolved.
        /// </summary>
        Resolving,
        /// <summary>
        /// Dependencies have been successfully resolved.
        /// </summary>
        Resolved,
        /// <summary>
        /// The last resolve operation was not successful.
        /// </summary>
        Faulted
    }
}