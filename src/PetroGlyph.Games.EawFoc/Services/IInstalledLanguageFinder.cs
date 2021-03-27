using System.Collections.Generic;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services
{
    public interface IInstalledLanguageFinder
    {
        ICollection<ILanguageInfo> FindInstalledLanguages(IPlayableObject playableObject);
    }

    public class FileSystemLanguageFinder : IInstalledLanguageFinder
    {
        public ICollection<ILanguageInfo> FindInstalledLanguages(IPlayableObject playableObject)
        {
            throw new System.NotImplementedException();
        }
    }
}
