using FluentAssertions;
using Infrastructure.Helpers;
using Xunit;

namespace Infrastructure.Tests.Helpers
{
    public sealed class EmbeddingJsonLoaderTests
    {
        [Fact]
        public void Load_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

            Action act = () => EmbeddingJsonLoader.Load(path);

            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void Load_ShouldThrowInvalidOperationException_WhenJsonIsEmpty()
        {
            var path = GetTestFilePath("empty.json");

            Action act = () => EmbeddingJsonLoader.Load(path);

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("No embeddings loaded");
        }

        [Fact]
        public void Load_ShouldThrowInvalidOperationException_WhenVectorLengthIsInvalid()
        {
            var path = GetTestFilePath("invalid-vector-length.json");

            Action act = () => EmbeddingJsonLoader.Load(path);

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Embedding for perfume 1 has invalid length 3");
        }

        [Fact]
        public void Load_ShouldLoadEmbeddings_WhenJsonIsValid()
        {
            var path = GetTestFilePath("valid-embeddings.json");

            var result = EmbeddingJsonLoader.Load(path);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(e => e.Vector.Length == 2048);
        }

        private static string GetTestFilePath(string fileName)
        {
            return Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                fileName
            );
        }
    }
}
