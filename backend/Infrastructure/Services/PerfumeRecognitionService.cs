using Core.DTOs;
using Core.Interfaces;
using PerfumeRecognition.Services;

namespace Infrastructure.Services;

public sealed class PerfumeRecognitionService : IPerfumeRecognitionService
{
    private readonly PerfumeRecognition.Services.PerfumeRecognitionService _mlService;

    public PerfumeRecognitionService(
        PerfumeRecognition.Services.PerfumeRecognitionService mlService)
    {
        _mlService = mlService;
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
            var results = _mlService.Recognize(tempPath, topK);

            return results
                .Select(r => new PerfumeRecognitionResultDto
                {
                    PerfumeId = r.PerfumeId,
                    Score = r.Score
                })
                .ToList();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
