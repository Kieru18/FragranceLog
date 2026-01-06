using Core.Interfaces;
using System.Globalization;
using System.Text.Json;

public sealed class GeoService : IGeoService
{
    private readonly HttpClient _http;

    public GeoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string?> ResolveCountryAsync(
        double lat,
        double lng,
        CancellationToken ct)
    {
        var latStr = lat.ToString(CultureInfo.InvariantCulture);
        var lngStr = lng.ToString(CultureInfo.InvariantCulture);

        var url =
            $"https://nominatim.openstreetmap.org/reverse" +
            $"?format=json" +
            $"&lat={latStr}" +
            $"&lon={lngStr}" +
            $"&zoom=3" +
            $"&addressdetails=1";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);

        req.Headers.UserAgent.ParseAdd("FragranceLog/1.0 (contact@fragrancelog.app)");
        req.Headers.Accept.ParseAdd("application/json");

        req.Headers.Add("Referer", "https://fragrancelog.app");
        req.Headers.Add("From", "contact@fragrancelog.app");

        var res = await _http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
            return null;

        using var stream = await res.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        if (!doc.RootElement.TryGetProperty("address", out var addr))
            return null;

        if (!addr.TryGetProperty("country_code", out var cc))
            return null;

        return cc.GetString()?.ToUpperInvariant();
    }
}
