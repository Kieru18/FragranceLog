using FluentAssertions;
using PerfumeRecognition.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace PerfumeRecognition.Tests.Services;

public sealed class LabHistogramColorDescriptorTests
{
    private static string SaveTemp(Image<Rgba32> image)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        image.Save(path);
        return path;
    }

    [Fact]
    public void Extract_returns_zero_vector_when_image_has_no_opaque_pixels()
    {
        using var img = new Image<Rgba32>(224, 224);
        var path = SaveTemp(img);

        var sut = new LabHistogramColorDescriptor();

        var hist = sut.Extract(path);

        hist.Length.Should().Be(sut.Dimension);
        hist.All(v => v == 0f).Should().BeTrue();
    }

    [Fact]
    public void Extract_returns_normalized_histogram_for_uniform_color()
    {
        using var img = new Image<Rgba32>(224, 224);
        for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
                img[x, y] = new Rgba32(255, 0, 0, 255);

        var path = SaveTemp(img);

        var sut = new LabHistogramColorDescriptor();

        var hist = sut.Extract(path);

        hist.Length.Should().Be(sut.Dimension);

        var l2 = MathF.Sqrt(hist.Sum(v => v * v));
        l2.Should().BeApproximately(1f, 1e-5f);

        hist.Count(v => v > 0f).Should().Be(1);
    }

    [Fact]
    public void Extract_ignores_transparent_pixels()
    {
        using var img = new Image<Rgba32>(224, 224);
        for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
                img[x, y] = new Rgba32(0, 255, 0, 0);

        for (int y = 80; y < 144; y++)
            for (int x = 80; x < 144; x++)
                img[x, y] = new Rgba32(0, 255, 0, 255);

        var path = SaveTemp(img);

        var sut = new LabHistogramColorDescriptor();

        var hist = sut.Extract(path);

        hist.All(v => v >= 0f).Should().BeTrue();

        var l2 = MathF.Sqrt(hist.Sum(v => v * v));
        l2.Should().BeApproximately(1f, 1e-5f);
    }

    [Fact]
    public void Extract_resizes_image_to_sample_size()
    {
        using var img = new Image<Rgba32>(100, 300);
        for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
                img[x, y] = new Rgba32(0, 0, 255, 255);

        var path = SaveTemp(img);

        var sut = new LabHistogramColorDescriptor(sampleSize: 224);

        var hist = sut.Extract(path);

        hist.Length.Should().Be(sut.Dimension);

        var l2 = MathF.Sqrt(hist.Sum(v => v * v));
        l2.Should().BeApproximately(1f, 1e-5f);
    }
}
