namespace DataImporter.Configuration;

public class ImportOptions
{
    public string CsvPath { get; set; } = null!;
    public int FakeUserCount { get; set; } = 50;
    public ImporterUserOptions ImporterUser { get; set; } = new();
}

public class ImporterUserOptions
{
    public string Password { get; set; } = null!;
}
