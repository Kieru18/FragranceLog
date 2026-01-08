using Microsoft.ML.Data;

namespace PerfumeRecognition.Models;

public sealed class ImageEmbedding
{
    [ColumnName("output")]
    [VectorType(2048)]
    public float[] Features { get; set; } = default!;
}
