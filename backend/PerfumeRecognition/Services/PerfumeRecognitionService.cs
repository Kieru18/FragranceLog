using PerfumeRecognition.Interfaces;
using PerfumeRecognition.Models;

namespace PerfumeRecognition.Services;

public sealed class PerfumeRecognitionService : IPerfumeRecognitionService
{
    private readonly IEmbeddingExtractor _extractor;
    private readonly IBackgroundRemover _backgroundRemover;
    private readonly IImageCropper _cropper;
    private readonly IColorDescriptorExtractor _colorExtractor;
    private readonly EmbeddingIndex _index;
    private readonly SimilaritySearch _search;

    public PerfumeRecognitionService(
        IEmbeddingExtractor extractor,
        IBackgroundRemover backgroundRemover,
        IImageCropper cropper,
        IColorDescriptorExtractor colorExtractor,
        EmbeddingIndex index,
        SimilaritySearch search)
    {
        _extractor = extractor;
        _backgroundRemover = backgroundRemover;
        _cropper = cropper;
        _colorExtractor = colorExtractor;
        _index = index;
        _search = search;
    }

    public IReadOnlyList<RecognitionResult> Recognize(
        string imagePath,
        int topK)
    {
        var backgroundRemovedPath = _backgroundRemover.Remove(imagePath);
        var croppedPath = _cropper.CropToForeground(backgroundRemovedPath);
        var embedding = _extractor.Extract(croppedPath);
        var color = _colorExtractor.Extract(croppedPath);

        return _search.RankWithColor(
            embedding,
            color,
            _index,
            topK);
    }
}
