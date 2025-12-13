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
    private readonly BrandAliasResolver _brandAlias;

    public ImageImporter(
        IOptions<ImportOptions> options,
        FragranceLogContext db,
        JsonlReader jsonl,
        NameNormalizer normalizer,
        PerfumeMatcher matcher,
        ImageFileService files,
        ImageDbWriterService writer,
        ReportWriter report,
        BrandAliasResolver brandAlias)
    {
        _options = options.Value;
        _db = db;
        _jsonl = jsonl;
        _normalizer = normalizer;
        _matcher = matcher;
        _files = files;
        _writer = writer;
        _report = report;
        _brandAlias = brandAlias;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var cfg = _options.Images;

        var projectRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..")
        );

        cfg.ImagesSourceFolder = Path.GetFullPath(Path.Combine(projectRoot, cfg.ImagesSourceFolder!));
        cfg.WebRootPerfumeImagesFolder = Path.GetFullPath(Path.Combine(projectRoot, cfg.WebRootPerfumeImagesFolder!));
        cfg.JsonlPath = Path.GetFullPath(Path.Combine(projectRoot, cfg.JsonlPath!));

        var dbPerfumes = await _db.Perfumes
            .Include(p => p.Brand)
            .ToListAsync(ct);

        var perfumeRows = dbPerfumes.Select(p =>
        {
            var normalized = _normalizer.NormalizeWithDiacritics(p.Name);
            var canonical = _normalizer.ExtractCanonicalName(normalized);

            return new DbPerfumeRow(
                p.PerfumeId,
                p.Brand.Name,
                p.Name,
                _brandAlias.Resolve(p.Brand.Name),
                normalized,
                canonical
            );
        }).ToList();

        var brandIndex = perfumeRows
            .GroupBy(p => p.BrandNormalized)
            .ToDictionary(g => g.Key, g => g.ToList());

        var datasetRecords = await _jsonl.ReadAsync(cfg.JsonlPath!);

        var existingPerfumeIds = (await _db.PerfumePhotos
            .Select(p => p.PerfumeId)
            .ToListAsync(ct))
            .ToHashSet();

        var matches = new List<MatchDecision>();
        var missingImages = new List<string>();

        foreach (var rec in datasetRecords)
        {
            var brandNorm = _brandAlias.Resolve(rec.Brand);
            var nameNorm = _normalizer.NormalizeWithDiacritics(rec.NamePerfume);
            var nameCanonical = _normalizer.ExtractCanonicalName(nameNorm);

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

            var canonicalMatches = candidates
                .Where(p => p.NameCanonical == nameCanonical)
                .ToList();

            if (canonicalMatches.Count >= 1)
            {
                matches.Add(MatchDecision.Canonical(rec, canonicalMatches));
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

        _files.EnsureDirectory(cfg.WebRootPerfumeImagesFolder!);

        int inserted = 0;

        foreach (var m in matches)
        {
            if (m.Kind == MatchKind.None)
                continue;

            if (string.IsNullOrWhiteSpace(m.Dataset.ImageName))
                continue;

            var src = Path.Combine(cfg.ImagesSourceFolder!, m.Dataset.ImageName);
            if (!_files.Exists(src))
            {
                missingImages.Add(m.Dataset.ImageName);
                continue;
            }

            var destName = Path.GetFileName(m.Dataset.ImageName);
            var dest = Path.Combine(cfg.WebRootPerfumeImagesFolder!, destName);

            if (!cfg.DoNotSave && !_files.Exists(dest))
                _files.Copy(src, dest);

            if (m.Kind == MatchKind.Canonical)
            {
                foreach (var db in m.DbPerfumes!)
                {
                    if (existingPerfumeIds.Contains(db.PerfumeId))
                        continue;

                    if (!cfg.DoNotSave)
                    {
                        await _writer.AddPhotoAsync(
                            db.PerfumeId,
                            $"{cfg.BasePathForDb}/{destName}"
                        );
                    }

                    existingPerfumeIds.Add(db.PerfumeId);
                    inserted++;
                }

                continue;
            }

            var perfumeId = m.DbPerfume!.PerfumeId;

            if (existingPerfumeIds.Contains(perfumeId))
                continue;

            if (!cfg.DoNotSave)
            {
                await _writer.AddPhotoAsync(
                    perfumeId,
                    $"{cfg.BasePathForDb}/{destName}"
                );
            }

            existingPerfumeIds.Add(perfumeId);
            inserted++;
        }

        var reportPath = Path.Combine("Output", "image-import-report.html");
        Directory.CreateDirectory("Output");
        await _report.WriteReportAsync(reportPath, matches, missingImages);
    }
}
