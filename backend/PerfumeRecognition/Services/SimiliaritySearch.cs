using PerfumeRecognition.Models;

namespace PerfumeRecognition.Services;

public sealed class SimilaritySearch
{
    public IReadOnlyList<RecognitionResult> FindTopK(
        float[] query,
        EmbeddingIndex index,
        int k)
    {
        return index.Items
            .Select(e => new RecognitionResult
            {
                PerfumeId = e.PerfumeId,
                Score = Cosine(query, e.Vector)
            })
            .OrderByDescending(x => x.Score)
            .Take(k)
            .ToList();
    }

    public IReadOnlyList<RecognitionResult> RankWithColor(
        float[] queryEmbedding,
        float[] queryColor,
        EmbeddingIndex index,
        int topK,
        int candidatePool = 10,
        float colorWeight = 0.25f)
    {
        var candidates = index.Items
            .Select(e => new
            {
                e.PerfumeId,
                Embedding = e.Vector,
                Color = e.Color,
                EmbeddingScore = Cosine(queryEmbedding, e.Vector)
            })
            .OrderByDescending(x => x.EmbeddingScore)
            .Take(candidatePool)
            .ToList();

        return candidates
            .Select(c =>
            {
                float colorScore = c.Color == null
                    ? 0f
                    : Cosine(queryColor, c.Color);

                return new RecognitionResult
                {
                    PerfumeId = c.PerfumeId,
                    Score =
                        (1f - colorWeight) * c.EmbeddingScore +
                        colorWeight * colorScore
                };
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();
    }

    private static float Cosine(float[] a, float[] b)
    {
        float dot = 0f;
        for (int i = 0; i < a.Length; i++)
            dot += a[i] * b[i];
        return dot;
    }
}
