using FluentAssertions;
using PerfumeRecognition.Services;
using Xunit;

namespace PerfumeRecognition.Tests.Services;

public sealed class EmbeddingNormalizerTests
{
    [Fact]
    public void Normalize_returns_same_vector_when_norm_is_zero()
    {
        var input = new float[] { 0f, 0f, 0f };

        var result = EmbeddingNormalizer.Normalize(input);

        result.Should().BeSameAs(input);
    }

    [Fact]
    public void Normalize_returns_unit_vector_for_non_zero_input()
    {
        var input = new float[] { 3f, 4f };

        var result = EmbeddingNormalizer.Normalize(input);

        var length = MathF.Sqrt(result[0] * result[0] + result[1] * result[1]);

        length.Should().BeApproximately(1f, 1e-5f);
    }

    [Fact]
    public void Normalize_does_not_modify_original_vector()
    {
        var input = new float[] { 1f, 2f, 3f };
        var copy = input.ToArray();

        _ = EmbeddingNormalizer.Normalize(input);

        input.Should().Equal(copy);
    }
}
