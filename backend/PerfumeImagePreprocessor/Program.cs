using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

static string FindSolutionRoot()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null)
    {
        if (dir.GetFiles("*.sln").Any())
            return dir.FullName;
        dir = dir.Parent;
    }
    throw new InvalidOperationException("Solution root not found");
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

var solutionRoot = FindSolutionRoot();

var imagesRoot = Path.GetFullPath(Path.Combine(
    solutionRoot,
    configuration["ImagePreprocessor:ImagesRoot"]!));

var outputRoot = Path.GetFullPath(Path.Combine(
    solutionRoot,
    configuration["ImagePreprocessor:OutputRoot"]!));

var modelPath = Path.GetFullPath(Path.Combine(
    solutionRoot,
    configuration["ImagePreprocessor:OnnxModelPath"]!));

Console.WriteLine($"SolutionRoot: {solutionRoot}");
Console.WriteLine($"ImagesRoot:   {imagesRoot}");
Console.WriteLine($"OutputRoot:   {outputRoot}");
Console.WriteLine($"ModelPath:    {modelPath}");

if (!File.Exists(modelPath))
    throw new FileNotFoundException("ONNX model not found", modelPath);

Directory.CreateDirectory(outputRoot);

var sessionOptions = new SessionOptions
{
    GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
    ExecutionMode = ExecutionMode.ORT_SEQUENTIAL
};

using var session = new InferenceSession(modelPath, sessionOptions);

using var db = new FragranceLogContext(
    new DbContextOptionsBuilder<FragranceLogContext>()
        .UseSqlServer(configuration.GetConnectionString("FragranceLogLocal"))
        .Options);

var photos = await db.PerfumePhotos
    .AsNoTracking()
    .Select(p => new { p.PhotoId, p.Path })
    .ToListAsync();

int processed = 0;
int skipped = 0;

foreach (var p in photos)
{
    Console.WriteLine("----");
    Console.WriteLine($"DB Path: {p.Path}");

    var ext = Path.GetExtension(p.Path).ToLowerInvariant();
    Console.WriteLine($"Extension: {ext}");

    if (ext == ".png" || (ext != ".jpg" && ext != ".jpeg"))
    {
        Console.WriteLine("Skipped due to extension");
        skipped++;
        continue;
    }

    var relativePath = p.Path.TrimStart('/', '\\');
    var inputPath = Path.Combine(imagesRoot, relativePath);
    Console.WriteLine($"Resolved path: {inputPath}");

    if (!File.Exists(inputPath))
    {
        Console.WriteLine("FILE NOT FOUND");
        skipped++;
        continue;
    }

    Console.WriteLine("FILE FOUND");

    using var resized = Image.Load<Rgb24>(inputPath);
    var originalSize = resized.Size;
    resized.Mutate(x => x.Resize(320, 320));

    var input = new DenseTensor<float>(new[] { 1, 3, 320, 320 });

    for (int y = 0; y < 320; y++)
    {
        for (int x = 0; x < 320; x++)
        {
            var px = resized[x, y];
            input[0, 0, y, x] = px.R / 255f;
            input[0, 1, y, x] = px.G / 255f;
            input[0, 2, y, x] = px.B / 255f;
        }
    }

    var inputName = session.InputMetadata.Keys.Single();
    var outputName = session.OutputMetadata.Keys.First();

    using var results = session.Run(new[]
    {
        NamedOnnxValue.CreateFromTensor(inputName, input)
    });

    var mask = results
        .First(r => r.Name == outputName)
        .AsTensor<float>();

    using var alpha = new Image<L8>(320, 320);
    for (int y = 0; y < 320; y++)
    {
        for (int x = 0; x < 320; x++)
        {
            var v = Math.Clamp(mask[0, 0, y, x], 0f, 1f);
            alpha[x, y] = new L8((byte)(v * 255));
        }
    }

    alpha.Mutate(x => x.Resize(originalSize.Width, originalSize.Height));

    using var original = Image.Load<Rgba32>(inputPath);
    for (int y = 0; y < original.Height; y++)
    {
        for (int x = 0; x < original.Width; x++)
        {
            var px = original[x, y];
            px.A = alpha[x, y].PackedValue;
            original[x, y] = px;
        }
    }

    var outputPath = Path.Combine(
        outputRoot,
        Path.ChangeExtension(Path.GetFileName(p.Path), ".png"));

    Console.WriteLine($"Output path: {outputPath}");

    original.SaveAsPng(outputPath);

    processed++;
    Console.WriteLine($"Processed {processed}");
}

Console.WriteLine($"DONE. Processed {processed}, skipped {skipped}");
