namespace DataImporter.Services
{
    public class ImageFileService
    {
        public bool Exists(string path) => File.Exists(path);

        public void EnsureDirectory(string path) => Directory.CreateDirectory(path);

        public void Copy(string src, string dst)
        {
            if (!File.Exists(dst))
                File.Copy(src, dst);
        }
    }
}
