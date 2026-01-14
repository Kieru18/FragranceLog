namespace PerfumeRecognition.Services
{
    public interface IImageCropper
    {
        string CropToForeground(string imagePath);
    }
}
