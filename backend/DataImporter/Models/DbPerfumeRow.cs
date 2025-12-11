namespace DataImporter.Models;

public sealed record DbPerfumeRow(
    int PerfumeId,
    string BrandName,
    string PerfumeName,
    string BrandNormalized,
    string NameNormalized
);
