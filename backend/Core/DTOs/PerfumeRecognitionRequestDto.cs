namespace Core.DTOs
{
    public sealed class PerfumeRecognitionRequestDto
    {
        public string ImageBase64 { get; set; } = null!;
        public int TopK { get; set; } = 3;
    }
}
