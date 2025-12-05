using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DataImporter.Models;

namespace DataImporter.Services;

public class CsvReaderService
{
    public List<KagglePerfumeRow> ReadRows(string path)
    {
        using var reader = new StreamReader(path);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            BadDataFound = null,
            MissingFieldFound = null
        };

        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<KagglePerfumeRow>().ToList();
        return records;
    }
}
