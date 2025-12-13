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

        // Convert underscores, hyphens, slashes into spaces
        s = Regex.Replace(s, @"[-_/]", " ");

        // Convert apostrophe-like chars into spaces
        s = Regex.Replace(s, @"['’´`]", " ");

        // Convert non alphanumerical chars into spaces
        s = Regex.Replace(s, @"[^a-z0-9]+", " ");

        // Collapse multiple spaces
        s = Regex.Replace(s, @"\s+", " ").Trim();

        return s;
    }

    public string ExtractCanonicalName(string normalized)
    {
        if (string.IsNullOrWhiteSpace(normalized))
            return "";

        // remove standalone years
        normalized = Regex.Replace(normalized, @"\b(19|20)\d{2}\b", "");

        // remove edition markers
        normalized = Regex.Replace(
            normalized,
            @"\b(limited|edition|collector|anniversary|special|reissue|release)\b",
            "",
            RegexOptions.IgnoreCase
        );

        // remove empty parentheses
        normalized = Regex.Replace(normalized, @"\(\s*\)", "");

        // collapse spaces
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        return normalized;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex RemoveSpacesRegex();
}
