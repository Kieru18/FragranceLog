using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerfumeRecognition.Services;
using SixLabors.ImageSharp;

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

var services = new ServiceCollection();

services.AddSingleton<IBackgroundRemover>(_ =>
    new BackgroundRemover(modelPath, outputRoot));

var provider = services.BuildServiceProvider();

var backgroundRemover = provider.GetRequiredService<IBackgroundRemover>();


if (!File.Exists(modelPath))
    throw new FileNotFoundException("ONNX model not found", modelPath);

Directory.CreateDirectory(outputRoot);


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
        Console.WriteLine($"Skipped due to extension: {ext}");
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

    string outputPath = backgroundRemover.Remove(inputPath);

    Console.WriteLine($"Output path: {outputPath}");

    processed++;
    Console.WriteLine($"Processed {processed}");
}

Console.WriteLine($"DONE. Processed {processed}, skipped {skipped}");
