using System.Text;
using Validation;

namespace ProductMetadata.Component
{
    internal static class Utilities
    {
        public static string FormatIdentity(string id, object? version, string? branch)
        {
            Requires.NotNull(id, nameof(id));
            var stringBuilder = new StringBuilder(id);
            if (version != null)
            {
                stringBuilder.Append(",version=");
                stringBuilder.Append(version);
            }
            if (!string.IsNullOrEmpty(branch))
            {
                stringBuilder.Append(",branch=");
                stringBuilder.Append(branch);
            }
            return stringBuilder.ToString();
        }
    }
}