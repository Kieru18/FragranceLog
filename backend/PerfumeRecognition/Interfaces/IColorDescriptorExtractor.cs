namespace PerfumeRecognition.Interfaces
{
    public interface IColorDescriptorExtractor
    {
        float[] Extract(string imagePath);
        int Dimension { get; }
    }
}
