using System.Text;
using System.Text.Json;
using DataImporter.Models;
using DataImporter.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataImporter.Tools;

public class BrandComparisonTool
{
    private readonly FragranceLogContext _db;
    private readonly JsonlReader _jsonl;
    private readonly NameNormalizer _normalizer;

    public BrandComparisonTool(
        FragranceLogContext db,
        JsonlReader jsonl,
        NameNormalizer normalizer)
    {
        _db = db;
        _jsonl = jsonl;
        _normalizer = normalizer;
    }

    public async Task RunAsync(string jsonlPath)
    {
        Console.WriteLine("=== BRAND COMPARISON TOOL ===");

        var dataset = await _jsonl.ReadAsync(jsonlPath);
        var datasetBrands = dataset
            .Select(r => r.Brand?.Trim() ?? "")
            .Where(b => b.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(b => b)
            .ToList();

        Console.WriteLine($"Dataset brands: {datasetBrands.Count}");

        var dbBrands = await _db.Brands
            .Select(b => b.Name)
            .ToListAsync();

        var dbNormalized = dbBrands
            .Select(b => new { Raw = b, Norm = _normalizer.NormalizeWithDiacritics(b) })
            .ToList();

        Console.WriteLine($"DB brands: {dbBrands.Count}");

        var reportRows = new List<BrandMatchRow>();

        foreach (var rawBrand in datasetBrands)
        {
            var normBrand = _normalizer.NormalizeWithDiacritics(rawBrand);

            var exact = dbNormalized.FirstOrDefault(b => b.Norm == normBrand);
            if (exact != null)
            {
                reportRows.Add(new BrandMatchRow(rawBrand, exact.Raw, "EXACT", 1.0));
                continue;
            }

            double bestScore = 0.0;
            string? bestDbBrand = null;

            foreach (var dbb in dbNormalized)
            {
                var score = Fuzzy(normBrand, dbb.Norm);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDbBrand = dbb.Raw;
                }
            }

            if (bestScore >= 0.92)
                reportRows.Add(new BrandMatchRow(rawBrand, bestDbBrand!, "HIGH-FUZZY", bestScore));
            else if (bestScore >= 0.82)
                reportRows.Add(new BrandMatchRow(rawBrand, bestDbBrand!, "LOW-FUZZY", bestScore));
            else
                reportRows.Add(new BrandMatchRow(rawBrand, null, "NO MATCH", bestScore));
        }

        WriteCsv(reportRows);
        WriteHtml(reportRows);

        Console.WriteLine("=== DONE ===");
    }

    private static double Fuzzy(string a, string b)
    {
        int dist = Levenshtein.Distance(a, b);
        int max = Math.Max(a.Length, b.Length);
        if (max == 0) return 1.0;
        return 1.0 - (double)dist / max;
    }

    private void WriteCsv(List<BrandMatchRow> rows)
    {
        var path = Path.Combine("Output", "brand-report.csv");
        Directory.CreateDirectory("Output");

        var sb = new StringBuilder();
        sb.AppendLine("DatasetBrand,DbBrand,Kind,Score");

        foreach (var r in rows)
        {
            sb.AppendLine($"{r.DatasetBrand},{r.DbBrand},{r.Kind},{r.Score:F3}");
        }

        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"CSV saved: {path}");
    }

    private void WriteHtml(List<BrandMatchRow> rows)
    {
        var path = Path.Combine("Output", "brand-report.html");

        var sb = new StringBuilder();
        sb.AppendLine("<html><body style='background:#111;color:#eee;font-family:sans-serif;'>");
        sb.AppendLine("<h1>Brand Comparison Report</h1>");
        sb.AppendLine("<table border='1' cellspacing='0' cellpadding='4' style='border-color:#333;'>");
        sb.AppendLine("<tr><th>Dataset Brand</th><th>DB Brand</th><th>Type</th><th>Score</th></tr>");

        foreach (var r in rows.OrderBy(r => r.Kind).ThenBy(r => r.DatasetBrand))
        {
            string color = r.Kind switch
            {
                "EXACT" => "#8fd98f",
                "HIGH-FUZZY" => "#ffd27f",
                "LOW-FUZZY" => "#ffb37f",
                _ => "#ff8f8f"
            };

            sb.AppendLine(
                $"<tr><td>{r.DatasetBrand}</td><td>{r.DbBrand}</td><td style='color:{color}'>{r.Kind}</td><td>{r.Score:F3}</td></tr>");
        }

        sb.AppendLine("</table></body></html>");

        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"HTML saved: {path}");
    }

    private record BrandMatchRow(string DatasetBrand, string? DbBrand, string Kind, double Score);
}
