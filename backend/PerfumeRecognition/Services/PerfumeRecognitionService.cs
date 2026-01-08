using PerfumeRecognition.Models;

namespace PerfumeRecognition.Services;

public sealed class PerfumeRecognitionService
{
    private readonly IEmbeddingExtractor _extractor;
    private readonly EmbeddingIndex _index;
    private readonly SimilaritySearch _search;

    public PerfumeRecognitionService(
        IEmbeddingExtractor extractor,
        EmbeddingIndex index,
        SimilaritySearch search)
    {
        _extractor = extractor;
        _index = index;
        _search = search;
    }

    public IReadOnlyList<RecognitionResult> Recognize(
        string imagePath,
        int topK)
    {
        var raw = _extractor.Extract(imagePath);
        var normalized = EmbeddingNormalizer.Normalize(raw);
        return _search.FindTopK(normalized, _index, topK);
    }
}
