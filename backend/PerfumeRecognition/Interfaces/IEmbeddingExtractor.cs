namespace PerfumeRecognition.Interfaces;

public interface IEmbeddingExtractor
{
    float[] Extract(string imagePath);
    int Dimension { get; }
}
