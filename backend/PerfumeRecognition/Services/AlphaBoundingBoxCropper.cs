using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PerfumeRecognition.Services;

public sealed class AlphaBoundingBoxCropper : IImageCropper
{
    private const int TargetSize = 224;

    public string CropToForeground(string imagePath)
    {
        using var image = Image.Load<Rgba32>(imagePath);

        int minX = image.Width, minY = image.Height;
        int maxX = -1, maxY = -1;

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                if (image[x, y].A > 10)
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

        if (maxX < minX || maxY < minY)
            return imagePath;

        int w = maxX - minX + 1;
        int h = maxY - minY + 1;

        if (w < 4 || h < 4)
            return imagePath;

        image.Mutate(ctx => ctx.Crop(new Rectangle(minX, minY, w, h)));

        float scale = Math.Min(
            (float)TargetSize / image.Width,
            (float)TargetSize / image.Height);

        int rw = Math.Max(1, (int)(image.Width * scale));
        int rh = Math.Max(1, (int)(image.Height * scale));

        image.Mutate(ctx => ctx.Resize(rw, rh));

        using var canvas = new Image<Rgba32>(TargetSize, TargetSize);
        int ox = (TargetSize - rw) / 2;
        int oy = (TargetSize - rh) / 2;

        canvas.Mutate(ctx => ctx.DrawImage(image, new Point(ox, oy), 1f));

        var outPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        canvas.Save(outPath);

        return outPath;
    }
}
