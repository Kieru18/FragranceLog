using Microsoft.Extensions.Configuration;
using DataImporter.Model;

namespace DataImporter;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
            
        var settings = config.GetSection("ImporterSettings").Get<ImporterSettings>();
        
        Console.WriteLine($"=== DATA IMPORTER ===");
        Console.WriteLine($"Running: {settings.ActiveImporter} Importer");
        
        var overseer = new ImportOverseer(settings);
        await overseer.RunActiveImporter();
        
        Console.WriteLine("Import completed!");
    }
}