namespace DataImporter.Importers;
using System.Net.Http;
using System.Threading.Tasks;

public abstract class BaseImporter
{
    protected readonly HttpClient _httpClient;
    
    protected BaseImporter()
    {
        _httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression =
                System.Net.DecompressionMethods.GZip |
                System.Net.DecompressionMethods.Deflate |
                System.Net.DecompressionMethods.Brotli
        });;
    }
    
    public abstract Task ImportData(string url);
    
    protected virtual async Task<string> GetHtmlAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

        request.Headers.TryAddWithoutValidation(
            "Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

        request.Headers.TryAddWithoutValidation(
            "Accept-Language",
            "en-US,en;q=0.5");

        request.Headers.Referrer = new Uri("https://www.google.com/");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await Task.Delay(Random.Shared.Next(800, 1500));

        return await response.Content.ReadAsStringAsync();
    }

}