using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using PerfumeRecognition.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PerfumeRecognition.Services
{
    public sealed class BackgroundRemover : IBackgroundRemover
    {
        private readonly InferenceSession _session;
        private readonly string _workDir;

        public BackgroundRemover(string modelPath, string workDir)
        {
            _session = new InferenceSession(modelPath, new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
                ExecutionMode = ExecutionMode.ORT_SEQUENTIAL
            });

            _workDir = workDir;
            Directory.CreateDirectory(_workDir);
        }

        public string Remove(string inputImagePath)
        {
            using var resized = Image.Load<Rgb24>(inputImagePath);
            var originalSize = resized.Size;

            resized.Mutate(x => x.Resize(320, 320));

            var input = new DenseTensor<float>(new[] { 1, 3, 320, 320 });

            for (int y = 0; y < 320; y++)
                for (int x = 0; x < 320; x++)
                {
                    var px = resized[x, y];
                    input[0, 0, y, x] = px.R / 255f;
                    input[0, 1, y, x] = px.G / 255f;
                    input[0, 2, y, x] = px.B / 255f;
                }

            var inputName = _session.InputMetadata.Keys.Single();
            var outputName = _session.OutputMetadata.Keys.First();

            using var results = _session.Run(new[]
            {
                NamedOnnxValue.CreateFromTensor(inputName, input)
            });

            var mask = results.First(r => r.Name == outputName).AsTensor<float>();

            using var alpha = new Image<L8>(320, 320);
            for (int y = 0; y < 320; y++)
                for (int x = 0; x < 320; x++)
                    alpha[x, y] = new L8((byte)(Math.Clamp(mask[0, 0, y, x], 0f, 1f) * 255));

            alpha.Mutate(x => x.Resize(originalSize.Width, originalSize.Height));

            using var original = Image.Load<Rgba32>(inputImagePath);
            for (int y = 0; y < original.Height; y++)
                for (int x = 0; x < original.Width; x++)
                {
                    var px = original[x, y];
                    px.A = alpha[x, y].PackedValue;
                    original[x, y] = px;
                }

            var outPath = Path.Combine(
                _workDir,
                Path.GetFileNameWithoutExtension(inputImagePath) + ".png");

            original.SaveAsPng(outPath);
            return outPath;
        }
    }
}
