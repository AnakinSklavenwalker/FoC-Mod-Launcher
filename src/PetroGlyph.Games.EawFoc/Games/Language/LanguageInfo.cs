using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Games.Language
{
    internal class LanguageInfo : ILanguageInfo
    {
        public string Code { get; }

        public LanguageSupportLevel Support { get; set; }

        public LanguageInfo(string code)
        {
            Code = code;
        }

        public bool Equals(ILanguageInfo other)
        {
            return Code == other.Code;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ILanguageInfo) obj);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }
}