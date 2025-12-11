using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DataImporter.Services;

public partial class NameNormalizer
{
    private static readonly TextInfo TextInfo =
        CultureInfo.InvariantCulture.TextInfo;

    public string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Name cannot be empty.");

        var withSpaces = raw.Replace('-', ' ');

        withSpaces = RemoveSpacesRegex().Replace(withSpaces, " ").Trim();

        return TextInfo.ToTitleCase(withSpaces.ToLowerInvariant());
    }

    public string NormalizeWithDiacritics(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return "";

        s = s.ToLowerInvariant().Trim();

        // Normalize unicode é → e, ç → c, ü → u, etc.
        s = s.Normalize(NormalizationForm.FormD);
        var chars = s.Where(c => CharUnicodeInfo.GetUnicodeCategory(c)
                                 != UnicodeCategory.NonSpacingMark);
        s = new string(chars.ToArray());

        // Standardize apostrophes, hyphens, slashes into spaces
        s = Regex.Replace(s, @"[-_/]", " ");

        // Remove apostrophes entirely
        s = Regex.Replace(s, @"['’´`]", "");

        // Convert non alphanumerical chars into spaces
        s = Regex.Replace(s, @"[^a-z0-9]+", " ");

        // Collapse multiple spaces
        s = Regex.Replace(s, @"\s+", " ").Trim();

        return s;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex RemoveSpacesRegex();
}
