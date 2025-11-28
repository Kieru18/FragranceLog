namespace DataImporter.Importers;

public class PerfumeImporter : BaseImporter  
{
    public override async Task ImportData(string url)
    {
        Console.WriteLine($"Importing perfumes from: {url}");
        // Implementation for perfumes
    }
}
