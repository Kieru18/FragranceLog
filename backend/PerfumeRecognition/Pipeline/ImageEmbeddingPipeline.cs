using Microsoft.ML;
using PerfumeRecognition.Models;

namespace PerfumeRecognition.Pipeline;

public static class ImageEmbeddingPipeline
{
    public static ITransformer Create(MLContext ml, string modelPath)
    {
        var pipeline =
            ml.Transforms.LoadImages(
                outputColumnName: "input",
                imageFolder: "",
                inputColumnName: nameof(ImageInput.ImagePath))
            .Append(ml.Transforms.ResizeImages(
                outputColumnName: "input",
                imageWidth: 224,
                imageHeight: 224))
            .Append(ml.Transforms.ExtractPixels(
                outputColumnName: "input",
                interleavePixelColors: true,
                scaleImage: 1f / 255f))
            .Append(ml.Transforms.ApplyOnnxModel(
                modelFile: modelPath,
                inputColumnNames: new[] { "input" },
                outputColumnNames: new[] { "embedding" }));

        var empty = ml.Data.LoadFromEnumerable(Array.Empty<ImageInput>());
        return pipeline.Fit(empty);
    }
}
