using PerfumeRecognition.Models;

namespace PerfumeRecognition.Services;

public sealed class EmbeddingIndex
{
    public IReadOnlyList<PerfumeEmbedding> Items { get; }

    public EmbeddingIndex(IEnumerable<PerfumeEmbedding> items)
    {
        Items = items.ToList();
    }
}
