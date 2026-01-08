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

    private static float Cosine(float[] a, float[] b)
    {
        float dot = 0f;
        for (int i = 0; i < a.Length; i++)
            dot += a[i] * b[i];
        return dot;
    }
}
