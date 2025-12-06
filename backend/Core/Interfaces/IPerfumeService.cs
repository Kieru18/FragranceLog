using Core.DTOs;

namespace Core.Interfaces
{
    public interface IPerfumeService
    {
        Task<PerfumeSearchResponseDto> SearchAsync(
            PerfumeSearchRequestDto request,
            CancellationToken ct);
    }
}
