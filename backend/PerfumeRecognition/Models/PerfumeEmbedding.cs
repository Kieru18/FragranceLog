namespace PerfumeRecognition.Models
{
    public sealed class PerfumeEmbedding
    {
        public int PerfumeId { get; set; }
        public float[] Vector { get; set; } = default!;
        public float[]? Color { get; set; }
    }
}
