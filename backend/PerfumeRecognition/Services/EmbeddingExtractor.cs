using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using PerfumeRecognition.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PerfumeRecognition.Services;

public sealed class EmbeddingExtractor : IEmbeddingExtractor, IDisposable
{
    private readonly InferenceSession _session;
    private readonly string _inputName;
    private readonly string _outputName;

    public int Dimension { get; } = 2048;

    public EmbeddingExtractor(string modelPath)
    {
        var options = new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            ExecutionMode = ExecutionMode.ORT_SEQUENTIAL
        };

        _session = new InferenceSession(modelPath, options);

        _inputName = _session.InputMetadata.Keys.Single();
        _outputName = _session.OutputMetadata.Keys.Single();
    }

    public float[] Extract(string imagePath)
    {
        var input = BuildInputTensor(imagePath);

        using var results = _session.Run(new[]
        {
            NamedOnnxValue.CreateFromTensor(_inputName, input)
        });

        var output = results.First(r => r.Name == _outputName).AsTensor<float>();

        var vector = new float[output.Length];
        output.ToArray().CopyTo(vector, 0);

        return vector;
    }

    private static DenseTensor<float> BuildInputTensor(string path)
    {
        using var image = Image.Load<Rgba32>(path);

        image.Mutate(x => x.Resize(224, 224));

        var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });

        for (int y = 0; y < 224; y++)
        {
            var row = image.DangerousGetPixelRowMemory(y).Span;
            for (int x = 0; x < 224; x++)
            {
                var px = row[x];
                tensor[0, 0, y, x] = px.R / 255f;
                tensor[0, 1, y, x] = px.G / 255f;
                tensor[0, 2, y, x] = px.B / 255f;
            }
        }

        return tensor;
    }

    public void Dispose()
    {
        _session.Dispose();
    }
}
