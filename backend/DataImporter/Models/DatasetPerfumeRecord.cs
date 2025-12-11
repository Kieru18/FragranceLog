using System.Text.Json.Serialization;

namespace DataImporter.Models;

public sealed record DatasetPerfumeRecord(
    [property: JsonPropertyName("brand")] string? Brand,
    [property: JsonPropertyName("name_perfume")] string? NamePerfume,
    [property: JsonPropertyName("family")] string? Family,
    [property: JsonPropertyName("subfamily")] string? Subfamily,
    [property: JsonPropertyName("fragrances")] string? Fragrances,
    [property: JsonPropertyName("ingredients")] List<string>? Ingredients,
    [property: JsonPropertyName("origin")] string? Origin,
    [property: JsonPropertyName("gender")] string? Gender,
    [property: JsonPropertyName("years")] string? Years,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("image_name")] string? ImageName
);

