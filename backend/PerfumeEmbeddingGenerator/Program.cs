using System.Text.Json;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PerfumeEmbeddingGenerator.Configuration;
using PerfumeRecognition.Models;
using PerfumeRecognition.Services;

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

var options = configuration
    .GetSection("EmbeddingGenerator")
    .Get<EmbeddingGeneratorOptions>()
    ?? throw new InvalidOperationException("Missing EmbeddingGenerator configuration");

var connectionString =
    configuration.GetConnectionString("FragranceLogLocal")
    ?? throw new InvalidOperationException("Missing connection string");

var solutionRoot = FindSolutionRoot();
var backendRoot = Path.Combine(solutionRoot, "backend");

var onnxModelPath = Path.Combine(backendRoot, options.OnnxModelPath);
var imagesRoot = Path.Combine(backendRoot, options.ImagesRoot);
var outputJsonPath = Path.Combine(backendRoot, options.OutputJsonPath);

if (!File.Exists(onnxModelPath))
    throw new FileNotFoundException("ONNX model not found", onnxModelPath);

using var db = new FragranceLogContext(
    new DbContextOptionsBuilder<FragranceLogContext>()
        .UseSqlServer(connectionString)
        .Options);

var extractor = new EmbeddingExtractor(onnxModelPath);

var photos = await db.PerfumePhotos
    .AsNoTracking()
    .Select(p => new { p.PerfumeId, p.Path })
    .ToListAsync();

var embeddings = new List<PerfumeEmbedding>();
int processed = 0;
int skipped = 0;

foreach (var p in photos)
{
    var relativePath = p.Path
        .TrimStart('\\', '/')
        .Replace('/', Path.DirectorySeparatorChar);

    if (relativePath.StartsWith("perfume-images" + Path.DirectorySeparatorChar))
        relativePath = relativePath.Substring("perfume-images".Length + 1);

    var imagePath = Path.Combine(imagesRoot, relativePath);

    if (!File.Exists(imagePath))
    {
        skipped++;
        continue;
    }

    try
    {
        var vector = extractor.Extract(imagePath);

        embeddings.Add(new PerfumeEmbedding
        {
            PerfumeId = p.PerfumeId,
            Vector = vector
        });

        processed++;

        if (processed % 100 == 0)
            Console.WriteLine($"Processed {processed}");
    }
    catch
    {
        skipped++;
    }
}

Directory.CreateDirectory(Path.GetDirectoryName(outputJsonPath)!);

File.WriteAllText(
    outputJsonPath,
    JsonSerializer.Serialize(embeddings));

Console.WriteLine($"DONE. Generated {processed}, skipped {skipped}");
