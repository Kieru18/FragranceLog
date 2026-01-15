using FluentAssertions;
using PerfumeRecognition.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace PerfumeRecognition.Tests.Services;

public sealed class EmbeddingExtractorTests
{
    private static string ResolveModelPath()
    {
        var baseDir = AppContext.BaseDirectory;

        return Path.GetFullPath(Path.Combine(
            baseDir,
            "..", "..", "..", "..",
            "PerfumeRecognition.Assets",
            "resnet101_ap_gem.onnx"));
    }

    [Fact]
    public void Extract_returns_vector_with_expected_dimension()
    {
        var modelPath = ResolveModelPath();
        if (!File.Exists(modelPath))
            return;

        var workDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(workDir);

        using var img = new Image<Rgba32>(128, 128);
        for (int y = 32; y < 96; y++)
            for (int x = 32; x < 96; x++)
                img[x, y] = new Rgba32(255, 255, 255, 255);

        var imagePath = Path.Combine(workDir, "input.png");
        img.Save(imagePath);

        using var sut = new EmbeddingExtractor(modelPath);

        var vector = sut.Extract(imagePath);

        vector.Should().NotBeNull();
        vector.Length.Should().Be(sut.Dimension);
        vector.Any(v => v != 0f).Should().BeTrue();
    }
}
