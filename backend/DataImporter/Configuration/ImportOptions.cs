namespace DataImporter.Configuration;

public class ImportOptions
{
    public string Mode { get; set; } = "Kaggle";
    public string ConnectionName { get; set; } = "FragranceLog";
    public KaggleImportOptions Kaggle { get; set; } = new();
    public ImageImportOptions Images { get; set; } = new();
}
