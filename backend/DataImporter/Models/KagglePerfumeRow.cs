using CsvHelper.Configuration.Attributes;

namespace DataImporter.Models;

public class KagglePerfumeRow
{
    [Name("url")]
    public string Url { get; set; } = null!;

    [Name("Perfume")]
    public string PerfumeName { get; set; } = null!;

    [Name("Brand")]
    public string BrandName { get; set; } = null!;

    [Name("Country")]
    public string? Country { get; set; }

    [Name("Gender")]
    public string? Gender { get; set; }

    [Name("Rating Value")]
    public string? RatingValueRaw { get; set; }

    [Name("Rating Count")]
    public string? RatingCountRaw { get; set; }

    [Name("Year")]
    public string? YearRaw { get; set; }

    [Name("Top")]
    public string? TopNotesRaw { get; set; }

    [Name("Middle")]
    public string? MiddleNotesRaw { get; set; }

    [Name("Base")]
    public string? BaseNotesRaw { get; set; }

    [Name("Perfumer1")]
    public string? Perfumer1 { get; set; }

    [Name("Perfumer2")]
    public string? Perfumer2 { get; set; }

    [Name("mainaccord1")]
    public string? MainAccord1 { get; set; }

    [Name("mainaccord2")]
    public string? MainAccord2 { get; set; }

    [Name("mainaccord3")]
    public string? MainAccord3 { get; set; }

    [Name("mainaccord4")]
    public string? MainAccord4 { get; set; }

    [Name("mainaccord5")]
    public string? MainAccord5 { get; set; }

    public IEnumerable<string> GetAccords()
    {
        var list = new[] { MainAccord1, MainAccord2, MainAccord3, MainAccord4, MainAccord5 };
        return list
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
