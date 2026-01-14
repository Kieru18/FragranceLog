using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using PerfumeRecognition.Services;

namespace Infrastructure.Services;

public sealed class PerfumeRecognitionService : Core.Interfaces.IPerfumeRecognitionService
{
    private readonly PerfumeRecognition.Services.IPerfumeRecognitionService _mlService;
    private readonly FragranceLogContext _dbContext;

    public PerfumeRecognitionService(
        PerfumeRecognition.Services.IPerfumeRecognitionService mlService,
        FragranceLogContext dbContext)
    {
        _mlService = mlService;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PerfumeRecognitionResultDto>> RecognizeAsync(
        Stream imageStream,
        int topK,
        CancellationToken ct)
    {
        var tempPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.jpg");

        await using (var fs = File.Create(tempPath))
        {
            await imageStream.CopyToAsync(fs, ct);
        }

        try
        {
            var mlResults = _mlService.Recognize(tempPath, topK);

            if (mlResults.Count == 0)
                return Array.Empty<PerfumeRecognitionResultDto>();

            var perfumeIds = mlResults
                .Select(r => r.PerfumeId)
                .Distinct()
                .ToArray();

            var metadata =
                from p in _dbContext.Perfumes.AsNoTracking()
                join photo in _dbContext.PerfumePhotos
                    on p.PerfumeId equals photo.PerfumeId into photos
                from photo in photos.DefaultIfEmpty()
                where perfumeIds.Contains(p.PerfumeId)
                select new
                {
                    p.PerfumeId,
                    p.Name,
                    BrandName = p.Brand.Name,
                    ImageUrl = photo.Path
                };

            var metaDict = await metadata
                .ToDictionaryAsync(x => x.PerfumeId, ct);

            return mlResults
                .Where(r => metaDict.ContainsKey(r.PerfumeId))
                .Select(r =>
                {
                    var meta = metaDict[r.PerfumeId];

                    return new PerfumeRecognitionResultDto
                    {
                        PerfumeId = r.PerfumeId,
                        Score = r.Score,
                        PerfumeName = meta.Name,
                        BrandName = meta.BrandName,
                        ImageUrl = meta.ImageUrl,
                        Confidence = MapConfidence(r.Score)
                    };
                })
                .ToList();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static PerfumeRecognitionConfidenceEnum MapConfidence(float score)
    {
        if (score >= 0.92f)
            return PerfumeRecognitionConfidenceEnum.High;

        if (score >= 0.85f)
            return PerfumeRecognitionConfidenceEnum.Medium;

        return PerfumeRecognitionConfidenceEnum.Low;
    }
}
