namespace DataImporter.Importers
{
    public interface IImporter
    {
        Task RunAsync(CancellationToken ct);
    }
}
