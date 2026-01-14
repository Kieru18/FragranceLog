namespace PerfumeRecognition.Services
{
    public interface IColorDescriptorExtractor
    {
        float[] Extract(string imagePath);
        int Dimension { get; }
    }
}
