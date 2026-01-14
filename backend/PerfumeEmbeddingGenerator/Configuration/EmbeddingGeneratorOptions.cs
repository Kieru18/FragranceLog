namespace PerfumeEmbeddingGenerator.Configuration;

public sealed class EmbeddingGeneratorOptions
{
    public string ImagesRoot { get; set; } = default!;
    public string OutputJsonPath { get; set; } = default!;
    public string OnnxModelPath { get; set; } = default!;
}
