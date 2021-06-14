using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public sealed class LanguageInfo : ILanguageInfo
    {
        public static readonly ILanguageInfo Default = EawModinfo.Model.LanguageInfo.Default;

        public string Code { get; }
        
        public LanguageSupportLevel Support { get; }

        public LanguageInfo(string code, LanguageSupportLevel support)
        {
            Code = code;
            Support = support;
        }
        
        public bool Equals(ILanguageInfo? other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Code == other.Code;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }
}
