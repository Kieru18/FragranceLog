using System;
using System.Collections.Generic;
using Core.Entities;

namespace DataImporter.Services;

public class CountryAliasResolver
{
    private static readonly Dictionary<string, string> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
    {
        { "Arabia saudi", "Saudi Arabia" },
        { "Bahrein", "Bahrain" },
        { "Czech Republic", "Czechia" },
        { "Russia", "Russian Federation" },
        { "Turkey", "Türkiye" },
        { "UAE", "United Arab Emirates" },
        { "UK", "United Kingdom of Great Britain and Northern Ireland" },
        { "USA", "United States of America" }
    };

    public Country? Resolve(
        string? rawValue,
        IReadOnlyDictionary<string, Country> countriesByName)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        var trimmed = rawValue.Trim();

        if (countriesByName.TryGetValue(trimmed, out var exact))
            return exact;

        if (Aliases.TryGetValue(trimmed, out var isoName) &&
            countriesByName.TryGetValue(isoName, out var mapped))
        {
            return mapped;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] Country '{trimmed}' not resolved.");
        Console.ResetColor();

        return null;
    }
}
