namespace DataImporter.Importers;
using System.Net.Http;
using System.Threading.Tasks;

public abstract class BaseImporter
{
    protected readonly HttpClient _httpClient;
    
    protected BaseImporter()
    {
        _httpClient = new HttpClient();
    }
    
    public abstract Task ImportData(string url);
    
    protected virtual async Task<string> GetHtmlAsync(string url)
    {
        return await _httpClient.GetStringAsync(url);
    }
}