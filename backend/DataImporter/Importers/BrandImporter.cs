namespace DataImporter.Importers;

public class BrandImporter : BaseImporter
{
    public override async Task ImportData(string url)
    {
        Console.WriteLine($"Importing brands from: {url}");
        // Implementation for brands
    }
}
