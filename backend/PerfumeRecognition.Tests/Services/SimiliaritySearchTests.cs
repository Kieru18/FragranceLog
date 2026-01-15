using FluentAssertions;
using PerfumeRecognition.Models;
using PerfumeRecognition.Services;
using Xunit;

namespace PerfumeRecognition.Tests.Services;

public sealed class SimilaritySearchTests
{
    [Fact]
    public void FindTopK_returns_items_ordered_by_cosine_score()
    {
        var index = new EmbeddingIndex(new[]
        {
            new PerfumeEmbedding { PerfumeId = 1, Vector = new[] { 1f, 0f } },
            new PerfumeEmbedding { PerfumeId = 2, Vector = new[] { 0f, 1f } },
            new PerfumeEmbedding { PerfumeId = 3, Vector = new[] { 0.8f, 0.2f } }
        });

        var query = new[] { 1f, 0f };
        var search = new SimilaritySearch();

        var result = search.FindTopK(query, index, 2);

        result.Should().HaveCount(2);
        result[0].PerfumeId.Should().Be(1);
        result[1].PerfumeId.Should().Be(3);
    }

    [Fact]
    public void FindTopK_respects_k_limit()
    {
        var index = new EmbeddingIndex(new[]
        {
            new PerfumeEmbedding { PerfumeId = 1, Vector = new[] { 1f } },
            new PerfumeEmbedding { PerfumeId = 2, Vector = new[] { 0.5f } }
        });

        var search = new SimilaritySearch();

        var result = search.FindTopK(new[] { 1f }, index, 1);

        result.Should().HaveCount(1);
    }

    [Fact]
    public void RankWithColor_uses_embedding_only_when_color_missing()
    {
        var index = new EmbeddingIndex(new[]
        {
            new PerfumeEmbedding
            {
                PerfumeId = 1,
                Vector = new[] { 1f, 0f },
                Color = null
            },
            new PerfumeEmbedding
            {
                PerfumeId = 2,
                Vector = new[] { 0.9f, 0.1f },
                Color = null
            }
        });

        var search = new SimilaritySearch();

        var result = search.RankWithColor(
            new[] { 1f, 0f },
            new[] { 1f, 0f },
            index,
            topK: 1);

        result.Should().HaveCount(1);
        result[0].PerfumeId.Should().Be(1);
    }

    [Fact]
    public void RankWithColor_applies_color_weight()
    {
        var index = new EmbeddingIndex(new[]
        {
            new PerfumeEmbedding
            {
                PerfumeId = 1,
                Vector = new[] { 1f, 0f },
                Color = new[] { 0f, 1f }
            },
            new PerfumeEmbedding
            {
                PerfumeId = 2,
                Vector = new[] { 0.9f, 0.1f },
                Color = new[] { 1f, 0f }
            }
        });

        var search = new SimilaritySearch();

        var result = search.RankWithColor(
            queryEmbedding: new[] { 1f, 0f },
            queryColor: new[] { 1f, 0f },
            index: index,
            topK: 1,
            candidatePool: 2,
            colorWeight: 0.5f);

        result.Should().HaveCount(1);
        result[0].PerfumeId.Should().Be(2);
    }

    [Fact]
    public void RankWithColor_respects_candidate_pool()
    {
        var index = new EmbeddingIndex(new[]
        {
            new PerfumeEmbedding { PerfumeId = 1, Vector = new[] { 1f, 0f }, Color = new[] { 0f, 1f } },
            new PerfumeEmbedding { PerfumeId = 2, Vector = new[] { 0.9f, 0.1f }, Color = new[] { 1f, 0f } },
            new PerfumeEmbedding { PerfumeId = 3, Vector = new[] { 0.8f, 0.2f }, Color = new[] { 1f, 0f } }
        });

        var search = new SimilaritySearch();

        var result = search.RankWithColor(
            new[] { 1f, 0f },
            new[] { 1f, 0f },
            index,
            topK: 2,
            candidatePool: 1);

        result.Should().HaveCount(1);
        result[0].PerfumeId.Should().Be(1);
    }
}
