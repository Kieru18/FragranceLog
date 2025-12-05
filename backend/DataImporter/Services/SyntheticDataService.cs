using System;
using System.Collections.Generic;

namespace DataImporter.Services;

public class SyntheticDataService
{
    /// <summary>
    /// Builds deterministic list of ratings (1–5) such that avg ≈ targetAvg.
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

    public Dictionary<string, int> BuildGenderDistribution(
        string? rawGender,
        int maxVotes
    )
    {
        if (maxVotes <= 0)
            return new();

        var normalized = rawGender?.Trim().ToLowerInvariant();

        Dictionary<string, double> ratio = normalized switch
        {
            "men" or "male" => new()
            {
                ["Male"] = 0.9,
                ["Unisex"] = 0.1
            },
            "women" or "female" => new()
            {
                ["Female"] = 0.9,
                ["Unisex"] = 0.1
            },
            "unisex" => new()
            {
                ["Unisex"] = 0.8,
                ["Male"] = 0.1,
                ["Female"] = 0.1
            },
            _ => new()
            {
                ["Unisex"] = 0.8,
                ["Male"] = 0.1,
                ["Female"] = 0.1
            }
        };

        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var assigned = 0;

        foreach (var kvp in ratio)
        {
            var votes = (int)Math.Round(kvp.Value * maxVotes);
            if (votes > 0)
            {
                result[kvp.Key] = votes;
                assigned += votes;
            }
        }

        while (assigned > maxVotes)
        {
            var key = result.Keys.First();
            result[key]--;
            assigned--;
        }

        while (assigned < maxVotes)
        {
            var key = result.Keys.First();
            result[key]++;
            assigned++;
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
