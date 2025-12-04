using System.Globalization;
using System.Text.RegularExpressions;

namespace DataImporter.Services;

public partial class NameNormalizer
{
    private static readonly TextInfo TextInfo =
        CultureInfo.InvariantCulture.TextInfo;

    public string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Brand name cannot be empty.");

        var withSpaces = raw.Replace('-', ' ');

        withSpaces = RemoveSpacesRegex().Replace(withSpaces, " ").Trim();

        return TextInfo.ToTitleCase(withSpaces.ToLowerInvariant());
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex RemoveSpacesRegex();
}
