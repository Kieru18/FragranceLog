namespace Core.Interfaces
{
    public interface IGeoService
    {
        Task<string?> ResolveCountryAsync(
            double latitude,
            double longitude,
            CancellationToken ct);
    }
}
