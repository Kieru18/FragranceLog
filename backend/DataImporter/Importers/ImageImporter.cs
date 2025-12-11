using AngleSharp.Dom;
using DataImporter.Configuration;
using DataImporter.Models;
using DataImporter.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DataImporter.Importers;

public class ImageImporter : IImporter
{
    private readonly ImportOptions _options;
    private readonly FragranceLogContext _db;
    private readonly JsonlReader _jsonl;
    private readonly NameNormalizer _normalizer;
    private readonly PerfumeMatcher _matcher;
    private readonly ImageFileService _files;
    private readonly ImageDbWriterService _writer;
    private readonly ReportWriter _report;

    public ImageImporter(
        IOptions<ImportOptions> options,
        FragranceLogContext db,
        JsonlReader jsonl,
        NameNormalizer normalizer,
        PerfumeMatcher matcher,
        ImageFileService files,
        ImageDbWriterService writer,
        ReportWriter report)
    {
        _options = options.Value;
        _db = db;
        _jsonl = jsonl;
        _normalizer = normalizer;
        _matcher = matcher;
        _files = files;
        _writer = writer;
        _report = report;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var cfg = _options.Images;

        Console.WriteLine("=== Image Importer (EF Core) ===");
        Console.WriteLine($"JSONL: {cfg.JsonlPath}");
        Console.WriteLine($"Images src: {cfg.ImagesSourceFolder}");
        Console.WriteLine($"Webroot dest: {cfg.WebRootPerfumeImagesFolder}");

        var dbPerfumes = await _db.Perfumes
            .Include(p => p.Brand)
            .ToListAsync();

        var perfumeRows = dbPerfumes.Select(p => new DbPerfumeRow(
            p.PerfumeId,
            p.Brand.Name,
            p.Name,
            _normalizer.NormalizeWithDiacritics(p.Brand.Name),
            _normalizer.NormalizeWithDiacritics(p.Name)
        )).ToList();

        var brandIndex = perfumeRows
            .GroupBy(p => p.BrandNormalized)
            .ToDictionary(g => g.Key, g => g.ToList());

        var datasetRecords = await _jsonl.ReadAsync(cfg.JsonlPath!);

        var existingPerfumeIds = (await _db.PerfumePhotos
            .Select(p => p.PerfumeId)
            .ToListAsync())
            .ToHashSet();

        var matches = new List<MatchDecision>();
        var missingImages = new List<string>();

        foreach (var rec in datasetRecords)
        {
            var brandNorm = _normalizer.NormalizeWithDiacritics(rec.Brand);
            var nameNorm = _normalizer.NormalizeWithDiacritics(rec.NamePerfume);

            if (string.IsNullOrWhiteSpace(brandNorm) || string.IsNullOrWhiteSpace(nameNorm))
            {
                matches.Add(MatchDecision.NoMatch(rec, "Cannot normalize brand or name"));
                continue;
            }

            if (!brandIndex.TryGetValue(brandNorm, out var candidates))
            {
                matches.Add(MatchDecision.NoMatch(rec, "Brand not found in DB"));
                continue;
            }

            var exact = candidates.FirstOrDefault(p => p.NameNormalized == nameNorm);
            if (exact != null)
            {
                matches.Add(MatchDecision.Exact(rec, exact));
                continue;
            }

            var fuzzy = _matcher.FindBestFuzzyMatch(rec, nameNorm, candidates);
            if (fuzzy != null)
            {
                matches.Add(fuzzy);
                continue;
            }

            matches.Add(MatchDecision.NoMatch(rec, "No exact or fuzzy match"));
        }

        Console.WriteLine("Matching complete.");
        Console.WriteLine($"Exact: {matches.Count(m => m.Kind == MatchKind.Exact)}");
        Console.WriteLine($"Fuzzy: {matches.Count(m => m.Kind == MatchKind.Fuzzy)}");
        Console.WriteLine($"None: {matches.Count(m => m.Kind == MatchKind.None)}");

        _files.EnsureDirectory(cfg.WebRootPerfumeImagesFolder!);

        int inserted = 0;
        int skippedExisting = 0;
        int skippedNoImage = 0;

        foreach (var m in matches.Where(m => m.Kind != MatchKind.None && m.DbPerfume != null))
        {
            var perfumeId = m.DbPerfume!.PerfumeId;

            if (existingPerfumeIds.Contains(perfumeId))
            {
                skippedExisting++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(m.Dataset.ImageName))
            {
                skippedNoImage++;
                continue;
            }

            var src = Path.Combine(cfg.ImagesSourceFolder!, m.Dataset.ImageName);
            if (!_files.Exists(src))
            {
                missingImages.Add(m.Dataset.ImageName!);
                skippedNoImage++;
                continue;
            }

            var ext = Path.GetExtension(src);
            if (string.IsNullOrWhiteSpace(ext))
                ext = ".jpg";

            var destName = perfumeId + ext.ToLowerInvariant();
            var dest = Path.Combine(cfg.WebRootPerfumeImagesFolder!, destName);

            if (cfg.DoNotSave) continue;

            _files.Copy(src, dest);

            await _writer.AddPhotoAsync(perfumeId, $"{cfg.BasePathForDb}/{destName}");

            existingPerfumeIds.Add(perfumeId);
            inserted++;
        }

        Console.WriteLine($"Inserted: {inserted}");
        Console.WriteLine($"Skipped existing: {skippedExisting}");
        Console.WriteLine($"Skipped no image: {skippedNoImage}");

        var reportPath = Path.Combine("Output", "image-import-report.html");
        Directory.CreateDirectory("Output");
        await _report.WriteReportAsync(reportPath, matches, missingImages);

        Console.WriteLine($"Report saved to {reportPath}");
    }
}
