using PerfumeRecognition.Models;
using System.Text.Json;

namespace Infrastructure.Helpers
{
    public static class EmbeddingJsonLoader
    {
        public static IReadOnlyList<PerfumeEmbedding> Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Embeddings JSON not found", filePath);

            var json = File.ReadAllText(filePath);

            var embeddings = JsonSerializer.Deserialize<List<PerfumeEmbedding>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (embeddings == null || embeddings.Count == 0)
                throw new InvalidOperationException("No embeddings loaded");

            foreach (var e in embeddings)
            {
                if (e.Vector.Length != 2048)
                    throw new InvalidOperationException(
                        $"Embedding for perfume {e.PerfumeId} has invalid length {e.Vector.Length}");
            }

            return embeddings;
        }
    }
}
