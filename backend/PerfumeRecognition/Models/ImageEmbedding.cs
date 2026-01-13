using Microsoft.ML.Data;

namespace PerfumeRecognition.Models;

public sealed class ImageEmbedding
{
    [ColumnName("embedding")]
    [VectorType(2048)]
    public float[] Features { get; set; } = default!;
}
