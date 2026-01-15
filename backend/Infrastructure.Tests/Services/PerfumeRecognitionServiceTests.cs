using Core.Enums;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.Data.Sqlite;
using Moq;
using PerfumeRecognition.Models;
using Xunit;
using PerfumeRecognition.Interfaces;

namespace Infrastructure.Tests.Services;

public sealed class PerfumeRecognitionServiceTests
{
    private readonly Mock<IPerfumeRecognitionService> _ml = new();

    private Infrastructure.Services.PerfumeRecognitionService CreateSut(FragranceLogContext ctx)
        => new(_ml.Object, ctx);

    private static MemoryStream CreateDummyImageStream()
        => new(new byte[] { 1, 2, 3, 4, 5 });

    private static string[] SnapshotTempJpgs()
        => Directory.GetFiles(Path.GetTempPath(), "*.jpg");

    private static string GetSingleNewFile(string[] before, string[] after)
    {
        var added = after.Except(before).ToList();
        added.Should().HaveCount(1);
        return added[0];
    }

    [Fact]
    public async Task RecognizeAsync_returns_empty_when_ml_returns_empty()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        _ml.Setup(x => x.Recognize(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Array.Empty<RecognitionResult>());

        var before = SnapshotTempJpgs();

        var rows = await CreateSut(ctx).RecognizeAsync(CreateDummyImageStream(), 5, default);

        rows.Should().BeEmpty();

        var after = SnapshotTempJpgs();
        after.Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task RecognizeAsync_enriches_results_with_metadata_and_maps_confidence_high()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithPhoto().Build();

        ctx.AddRange(brand, p1);

        if (p1.PerfumePhoto != null)
            ctx.Add(p1.PerfumePhoto);

        await ctx.SaveChangesAsync();

        _ml.Setup(x => x.Recognize(It.IsAny<string>(), 3))
            .Returns(new[]
            {
                new RecognitionResult { PerfumeId = 1, Score = 0.92f }
            });

        var before = SnapshotTempJpgs();

        var rows = await CreateSut(ctx).RecognizeAsync(CreateDummyImageStream(), 3, default);

        rows.Should().HaveCount(1);
        rows[0].PerfumeId.Should().Be(1);
        rows[0].Score.Should().Be(0.92f);
        rows[0].PerfumeName.Should().Be(p1.Name);
        rows[0].BrandName.Should().Be(brand.Name);
        rows[0].ImageUrl.Should().NotBeNull();
        rows[0].Confidence.Should().Be(PerfumeRecognitionConfidenceEnum.High);

        var after = SnapshotTempJpgs();
        after.Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task RecognizeAsync_maps_confidence_medium_and_low_at_boundaries()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();

        var pHighBoundary = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();
        var pMedBoundary = PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build();
        var pLow = PerfumeBuilder.Default().WithId(3).WithBrand(brand).Build();

        ctx.AddRange(brand, pHighBoundary, pMedBoundary, pLow);
        await ctx.SaveChangesAsync();

        _ml.Setup(x => x.Recognize(It.IsAny<string>(), 10))
            .Returns(new[]
            {
                new RecognitionResult { PerfumeId = 1, Score = 0.92f },
                new RecognitionResult { PerfumeId = 2, Score = 0.85f },
                new RecognitionResult { PerfumeId = 3, Score = 0.8499f }
            });

        var rows = await CreateSut(ctx).RecognizeAsync(CreateDummyImageStream(), 10, default);

        rows.Should().HaveCount(3);
        rows.Single(x => x.PerfumeId == 1).Confidence.Should().Be(PerfumeRecognitionConfidenceEnum.High);
        rows.Single(x => x.PerfumeId == 2).Confidence.Should().Be(PerfumeRecognitionConfidenceEnum.Medium);
        rows.Single(x => x.PerfumeId == 3).Confidence.Should().Be(PerfumeRecognitionConfidenceEnum.Low);
    }

    [Fact]
    public async Task RecognizeAsync_filters_out_results_when_perfume_metadata_missing()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var existing = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();

        ctx.AddRange(brand, existing);
        await ctx.SaveChangesAsync();

        _ml.Setup(x => x.Recognize(It.IsAny<string>(), 5))
            .Returns(new[]
            {
                new RecognitionResult { PerfumeId = 999, Score = 0.99f },
                new RecognitionResult { PerfumeId = 1, Score = 0.88f }
            });

        var rows = await CreateSut(ctx).RecognizeAsync(CreateDummyImageStream(), 5, default);

        rows.Should().HaveCount(1);
        rows[0].PerfumeId.Should().Be(1);
    }

    [Fact]
    public async Task RecognizeAsync_preserves_ml_order_not_db_order()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build();
        var p3 = PerfumeBuilder.Default().WithId(3).WithBrand(brand).Build();

        ctx.AddRange(brand, p1, p2, p3);
        await ctx.SaveChangesAsync();

        _ml.Setup(x => x.Recognize(It.IsAny<string>(), 3))
            .Returns(new[]
            {
                new RecognitionResult { PerfumeId = 3, Score = 0.90f },
                new RecognitionResult { PerfumeId = 1, Score = 0.89f },
                new RecognitionResult { PerfumeId = 2, Score = 0.88f }
            });

        var rows = await CreateSut(ctx).RecognizeAsync(CreateDummyImageStream(), 3, default);

        rows.Select(x => x.PerfumeId).Should().Equal(3, 1, 2);
    }

    [Fact]
    public async Task RecognizeAsync_sets_imageurl_null_when_no_photo_exists()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var p = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();

        ctx.AddRange(brand, p);
        await ctx.SaveChangesAsync();

        _ml.Setup(x => x.Recognize(It.IsAny<string>(), 1))
            .Returns(new[]
            {
                new RecognitionResult { PerfumeId = 1, Score = 0.93f }
            });

        var rows = await CreateSut(ctx).RecognizeAsync(CreateDummyImageStream(), 1, default);

        rows.Should().HaveCount(1);
        rows[0].ImageUrl.Should().BeNull();
    }

    [Fact]
    public async Task RecognizeAsync_deletes_temp_file_even_when_ml_throws()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        string? capturedPath = null;

        _ml.Setup(x => x.Recognize(It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, int>((path, _) => capturedPath = path)
            .Throws(new InvalidOperationException("boom"));

        var before = SnapshotTempJpgs();

        await CreateSut(ctx).Invoking(x => x.RecognizeAsync(CreateDummyImageStream(), 5, default))
            .Should()
            .ThrowAsync<InvalidOperationException>();

        capturedPath.Should().NotBeNull();
        File.Exists(capturedPath!).Should().BeFalse();

        var after = SnapshotTempJpgs();
        after.Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task RecognizeAsync_passes_topK_to_ml_service()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        int? receivedTopK = null;

        _ml.Setup(x => x.Recognize(It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, int>((_, topK) => receivedTopK = topK)
            .Returns(Array.Empty<RecognitionResult>());

        await CreateSut(ctx).RecognizeAsync(CreateDummyImageStream(), 7, default);

        receivedTopK.Should().Be(7);
    }
}
