using Core.DTOs;

namespace Core.Interfaces;

public interface IPerfumeRecognitionService
{
    Task<IReadOnlyList<PerfumeRecognitionResultDto>> RecognizeAsync(
        Stream imageStream,
        int topK,
        CancellationToken ct);
}
