namespace DataImporter.Configuration
{
    public class ImageImportOptions
    {
        public string JsonlPath { get; set; } = "";
        public string ImagesSourceFolder { get; set; } = "";
        public string WebRootPerfumeImagesFolder { get; set; } = "";
        public string BasePathForDb { get; set; } = "";
        public int MaxReviewsInReport { get; set; } = 5;

        public bool DoNotSave { get; set; } = false;
    }
}
