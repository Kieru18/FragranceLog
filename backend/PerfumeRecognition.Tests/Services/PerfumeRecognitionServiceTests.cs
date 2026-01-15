using FluentAssertions;
using Moq;
using PerfumeRecognition.Interfaces;
using PerfumeRecognition.Models;
using PerfumeRecognition.Services;
using Xunit;

namespace PerfumeRecognition.Tests.Services;

public sealed class PerfumeRecognitionServiceTests
{
    [Fact]
    public void Recognize_executes_full_pipeline_and_returns_results()
    {
        var extractor = new Mock<IEmbeddingExtractor>();
        var backgroundRemover = new Mock<IBackgroundRemover>();
        var cropper = new Mock<IImageCropper>();
        var colorExtractor = new Mock<IColorDescriptorExtractor>();

        var index = new EmbeddingIndex(new[]
        {
            new PerfumeEmbedding
            {
                PerfumeId = 1,
                Vector = new float[] { 1f, 0f }
            }
        });

        var search = new SimilaritySearch();

        var embedding = new float[] { 1f, 0f };
        var color = new float[] { 1f, 0f, 0f };

        backgroundRemover
            .Setup(x => x.Remove("input.jpg"))
            .Returns("bg.png");

        cropper
            .Setup(x => x.CropToForeground("bg.png"))
            .Returns("crop.png");

        extractor
            .Setup(x => x.Extract("crop.png"))
            .Returns(embedding);

        colorExtractor
            .Setup(x => x.Extract("crop.png"))
            .Returns(color);

        var sut = new PerfumeRecognitionService(
            extractor.Object,
            backgroundRemover.Object,
            cropper.Object,
            colorExtractor.Object,
            index,
            search);

        var output = sut.Recognize("input.jpg", 3);

        output.Should().HaveCount(1);
        output[0].PerfumeId.Should().Be(1);

        backgroundRemover.Verify(x => x.Remove("input.jpg"), Times.Once);
        cropper.Verify(x => x.CropToForeground("bg.png"), Times.Once);
        extractor.Verify(x => x.Extract("crop.png"), Times.Once);
        colorExtractor.Verify(x => x.Extract("crop.png"), Times.Once);
    }
}
