using Microsoft.ML;
using PerfumeRecognition.Models;
using PerfumeRecognition.Pipeline;

namespace PerfumeRecognition.Services;

public sealed class EmbeddingExtractor : IEmbeddingExtractor
{
    private readonly PredictionEngine<ImageInput, ImageEmbedding> _engine;

    public EmbeddingExtractor(string modelPath)
    {
        var (ml, model) = PipelineFactory.Get(modelPath);
        _engine = ml.Model.CreatePredictionEngine<ImageInput, ImageEmbedding>(model);
    }

    public float[] Extract(string imagePath)
    {
        var result = _engine.Predict(new ImageInput
        {
            ImagePath = imagePath
        });

        return result.Features;
    }
}
