using FluentAssertions;
using PerfumeRecognition.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace PerfumeRecognition.Tests.Services;

public sealed class AlphaBoundingBoxCropperTests
{
    private static string SaveTemp(Image<Rgba32> image)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        image.Save(path);
        return path;
    }

    private static void DrawOpaqueRect(
        Image<Rgba32> img,
        int x,
        int y,
        int w,
        int h)
    {
        for (int iy = y; iy < y + h; iy++)
            for (int ix = x; ix < x + w; ix++)
                img[ix, iy] = new Rgba32(255, 255, 255, 255);
    }

    [Fact]
    public void Returns_original_path_when_image_is_fully_transparent()
    {
        using var img = new Image<Rgba32>(100, 100);
        var path = SaveTemp(img);

        var sut = new AlphaBoundingBoxCropper();

        var result = sut.CropToForeground(path);

        result.Should().Be(path);
    }

    [Fact]
    public void Returns_original_path_when_foreground_is_too_small()
    {
        using var img = new Image<Rgba32>(100, 100);
        img[50, 50] = new Rgba32(255, 255, 255, 255);

        var path = SaveTemp(img);

        var sut = new AlphaBoundingBoxCropper();

        var result = sut.CropToForeground(path);

        result.Should().Be(path);
    }

    [Fact]
    public void Crops_and_resizes_normal_foreground()
    {
        using var img = new Image<Rgba32>(200, 200);
        DrawOpaqueRect(img, 50, 60, 80, 90);

        var path = SaveTemp(img);

        var sut = new AlphaBoundingBoxCropper();

        var result = sut.CropToForeground(path);

        result.Should().NotBe(path);
        File.Exists(result).Should().BeTrue();

        using var output = Image.Load<Rgba32>(result);
        output.Width.Should().Be(224);
        output.Height.Should().Be(224);
    }

    [Fact]
    public void Handles_foreground_touching_edges()
    {
        using var img = new Image<Rgba32>(200, 200);
        DrawOpaqueRect(img, 0, 0, 100, 200);

        var path = SaveTemp(img);

        var sut = new AlphaBoundingBoxCropper();

        var result = sut.CropToForeground(path);

        result.Should().NotBe(path);

        using var output = Image.Load<Rgba32>(result);
        output.Width.Should().Be(224);
        output.Height.Should().Be(224);
    }

    [Fact]
    public void Preserves_aspect_ratio_for_non_square_foreground()
    {
        using var img = new Image<Rgba32>(300, 150);
        DrawOpaqueRect(img, 50, 25, 200, 50);

        var path = SaveTemp(img);

        var sut = new AlphaBoundingBoxCropper();

        var result = sut.CropToForeground(path);

        using var output = Image.Load<Rgba32>(result);
        output.Width.Should().Be(224);
        output.Height.Should().Be(224);
    }
}
