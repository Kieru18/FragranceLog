using Microsoft.ML;

namespace PerfumeRecognition.Pipeline;

public static class PipelineFactory
{
    private static readonly object _lock = new();
    private static ITransformer? _model;
    private static MLContext? _ml;

    public static (MLContext ml, ITransformer model) Get(string modelPath)
    {
        lock (_lock)
        {
            if (_model != null && _ml != null)
                return (_ml, _model);

            _ml = new MLContext(seed: 1);
            _model = ImageEmbeddingPipeline.Create(_ml, modelPath);
            return (_ml, _model);
        }
    }
}
