using Core.DTOs;

namespace Core.Interfaces
{
    public interface IPerfumeService
    {
        Task<PerfumeSearchResponseDto> SearchAsync(
            PerfumeSearchRequestDto request,
            CancellationToken ct);

        Task<PerfumeDetailsDto> GetDetailsAsync(
            int perfumeId,
            int? userId,
            CancellationToken ct);
    }
}
