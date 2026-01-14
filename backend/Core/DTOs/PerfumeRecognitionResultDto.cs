using Core.Enums;

namespace Core.DTOs;

public sealed class PerfumeRecognitionResultDto
{
    public int PerfumeId { get; set; }
    public float Score { get; set; }

    public string PerfumeName { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public string? ImageUrl { get; set; }

    public PerfumeRecognitionConfidenceEnum Confidence { get; set; }
}
