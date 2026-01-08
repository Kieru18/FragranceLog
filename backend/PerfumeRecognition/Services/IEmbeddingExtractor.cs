namespace PerfumeRecognition.Services;

public interface IEmbeddingExtractor
{
    float[] Extract(string imagePath);
}
