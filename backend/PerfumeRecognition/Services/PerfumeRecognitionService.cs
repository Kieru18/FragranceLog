using PerfumeRecognition.Models;

namespace PerfumeRecognition.Services;

public sealed class PerfumeRecognitionService
{
    private readonly IEmbeddingExtractor _extractor;
    private readonly IBackgroundRemover _backgroundRemover;
    private readonly EmbeddingIndex _index;
    private readonly SimilaritySearch _search;

    public PerfumeRecognitionService(
        IEmbeddingExtractor extractor,
        IBackgroundRemover backgroundRemover,
        EmbeddingIndex index,
        SimilaritySearch search)
    {
        _extractor = extractor;
        _backgroundRemover = backgroundRemover;
        _index = index;
        _search = search;
    }

    public IReadOnlyList<RecognitionResult> Recognize(
        string imagePath,
        int topK)
    {
        var processedPath = _backgroundRemover.Remove(imagePath);
        var embedding = _extractor.Extract(processedPath);
        return _search.FindTopK(embedding, _index, topK);
    }
}
