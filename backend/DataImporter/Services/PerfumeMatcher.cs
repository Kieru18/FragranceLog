using DataImporter.Models;

namespace DataImporter.Services;

public class PerfumeMatcher
{
    private readonly NameNormalizer _normalizer;

    public PerfumeMatcher(NameNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    /// <summary>
    /// Perform fuzzy match within the same brand.
    /// </summary>
    public MatchDecision? FindBestFuzzyMatch(
        DatasetPerfumeRecord dataset,
        string normalizedName,
        List<DbPerfumeRow> candidates)
    {
        if (candidates.Count == 0)
            return null;

        double bestScore = 0;
        DbPerfumeRow? best = null;

        foreach (var c in candidates)
        {
            var dist = Levenshtein.Distance(normalizedName, c.NameNormalized);
            var maxLen = Math.Max(normalizedName.Length, c.NameNormalized.Length);
            if (maxLen == 0) continue;

            var score = 1.0 - (double)dist / maxLen;

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }

        const double threshold = 0.92;
        if (best == null || bestScore < threshold)
            return null;

        return MatchDecision.Fuzzy(dataset, best, bestScore);
    }
}

public static class Levenshtein
{
    public static int Distance(string s, string t)
    {
        if (s == t) return 0;
        if (s.Length == 0) return t.Length;
        if (t.Length == 0) return s.Length;

        int[,] d = new int[s.Length + 1, t.Length + 1];

        for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++) d[0, j] = j;

        for (int i = 1; i < d.GetLength(0); i++)
        {
            for (int j = 1; j < d.GetLength(1); j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }

        return d[s.Length, t.Length];
    }
}
