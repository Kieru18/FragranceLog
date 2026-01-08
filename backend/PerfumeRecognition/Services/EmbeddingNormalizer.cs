namespace PerfumeRecognition.Services;

public static class EmbeddingNormalizer
{
    public static float[] Normalize(float[] vector)
    {
        float sum = 0f;
        for (int i = 0; i < vector.Length; i++)
            sum += vector[i] * vector[i];

        var norm = MathF.Sqrt(sum);
        if (norm == 0f)
            return vector;

        var result = new float[vector.Length];
        for (int i = 0; i < vector.Length; i++)
            result[i] = vector[i] / norm;

        return result;
    }
}
