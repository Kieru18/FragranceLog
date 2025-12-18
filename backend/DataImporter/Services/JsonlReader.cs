using DataImporter.Models;
using System.Text;
using System.Text.Json;

namespace DataImporter.Services;

public class JsonlReader
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<DatasetPerfumeRecord>> ReadAsync(string path)
    {
        var list = new List<DatasetPerfumeRecord>();

        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var rec = JsonSerializer.Deserialize<DatasetPerfumeRecord>(line, _options);
                if (rec != null)
                    list.Add(rec);
            }
            catch
            {
                // Ignore malformed lines
            }
        }

        return list;
    }
}
