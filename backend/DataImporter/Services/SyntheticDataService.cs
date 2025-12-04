using System;
using System.Collections.Generic;

namespace DataImporter.Services;

public class SyntheticDataService
{
    /// <summary>
    /// Build deterministic list of ratings (1–5) such that avg ≈ targetAvg.
    /// </summary>
    public IReadOnlyList<int> BuildRatings(double targetAvg, int count)
    {
        var result = new int[count];
        if (count <= 0 || targetAvg <= 0)
            return result;

        var clampedAvg = Math.Max(1.0, Math.Min(5.0, targetAvg));
        var baseRating = (int)Math.Round(clampedAvg, MidpointRounding.AwayFromZero);
        baseRating = Math.Clamp(baseRating, 1, 5);

        var totalTarget = (int)Math.Round(clampedAvg * count, MidpointRounding.AwayFromZero);
        var currentTotal = baseRating * count;
        var diff = totalTarget - currentTotal;

        for (int i = 0; i < count; i++)
        {
            result[i] = baseRating;
        }

        var idx = 0;
        while (diff != 0 && count > 0)
        {
            if (diff > 0 && result[idx] < 5)
            {
                result[idx]++;
                diff--;
            }
            else if (diff < 0 && result[idx] > 1)
            {
                result[idx]--;
                diff++;
            }

            idx = (idx + 1) % count;
        }

        return result;
    }

    /// <summary>
    /// Map CSV gender string to logical Gender.Name in DB.
    /// </summary>
    public string MapGenderName(string? rawGender)
    {
        if (string.IsNullOrWhiteSpace(rawGender))
            return "Unisex";

        var g = rawGender.Trim().ToLowerInvariant();
        return g switch
        {
            "men" or "male" => "Male",
            "women" or "female" => "Female",
            "unisex" => "Unisex",
            _ => "Unisex"
        };
    }
}
