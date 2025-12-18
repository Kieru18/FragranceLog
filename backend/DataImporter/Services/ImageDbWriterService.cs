using Core.Entities;
using Infrastructure.Data;

namespace DataImporter.Services
{
    public class ImageDbWriterService
    {
        private readonly FragranceLogContext _db;

        public ImageDbWriterService(FragranceLogContext db)
        {
            _db = db;
        }

        public async Task AddPhotoAsync(int perfumeId, string dbPath)
        {
            var photo = new PerfumePhoto
            {
                PerfumeId = perfumeId,
                Path = dbPath,
                Description = "Imported from https://huggingface.co/datasets/doevent/perfume"
            };

            _db.PerfumePhotos.Add(photo);
            await _db.SaveChangesAsync();
        }
    }
}
