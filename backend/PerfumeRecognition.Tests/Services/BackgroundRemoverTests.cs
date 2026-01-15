using FluentAssertions;
using PerfumeRecognition.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace PerfumeRecognition.Tests.Services;

public sealed class BackgroundRemoverTests
{
    private static string ResolveModelPath()
    {
        var baseDir = AppContext.BaseDirectory;

        var path = Path.GetFullPath(Path.Combine(
            baseDir,
            "..", "..", "..", "..",
            "PerfumeImagePreprocessor.Assets",
            "u2net.onnx"));

        return path;
    }

    [Fact]
    public void Remove_produces_png_with_alpha_and_preserves_dimensions()
    {
        var modelPath = ResolveModelPath();
        if (!File.Exists(modelPath))
            return;

        var workDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(workDir);

        using var img = new Image<Rgba32>(128, 96);
        for (int y = 20; y < 80; y++)
            for (int x = 30; x < 100; x++)
                img[x, y] = new Rgba32(255, 0, 0, 255);

        var inputPath = Path.Combine(workDir, "input.png");
        img.Save(inputPath);

        var sut = new BackgroundRemover(modelPath, workDir);

        var outputPath = sut.Remove(inputPath);

        File.Exists(outputPath).Should().BeTrue();

        using var output = Image.Load<Rgba32>(outputPath);
        output.Width.Should().Be(128);
        output.Height.Should().Be(96);

        var hasTransparentPixel = false;
        for (int y = 0; y < output.Height && !hasTransparentPixel; y++)
            for (int x = 0; x < output.Width && !hasTransparentPixel; x++)
                if (output[x, y].A < 255)
                    hasTransparentPixel = true;

        hasTransparentPixel.Should().BeTrue();
    }
}
