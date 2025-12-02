using Microsoft.Extensions.Configuration;
using DataImporter.Model;

namespace DataImporter;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
            
        var settings = config.GetSection("ImporterSettings").Get<ImporterSettings>();
        
        Console.WriteLine($"=== DATA IMPORTER ===");
        Console.WriteLine($"Running: {settings.ActiveImporter} Importer");
        
        var overseer = new ImportOverseer(settings);
        await overseer.RunActiveImporter();
        
        Console.WriteLine("Import completed!");
    }
}