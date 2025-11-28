using DataImporter.Model;
using DataImporter.Importers;

namespace DataImporter;

public class ImportOverseer
{
    private readonly ImporterSettings _settings;
    private readonly NoteImporter _noteImporter;
    private readonly BrandImporter _brandImporter;
    private readonly PerfumeImporter _perfumeImporter;

    public ImportOverseer(ImporterSettings settings)
    {
        _settings = settings;
        _noteImporter = new NoteImporter();
        _brandImporter = new BrandImporter();
        _perfumeImporter = new PerfumeImporter();
    }

    public async Task RunActiveImporter()
    {
        switch (_settings.ActiveImporter.ToLower())
        {
            case "notes":
                await _noteImporter.ImportData(_settings.Urls.Notes);
                break;
            case "brands":
                await _brandImporter.ImportData(_settings.Urls.Brands);
                break;
            case "perfumes":
                await _perfumeImporter.ImportData(_settings.Urls.Perfumes);
                break;
            default:
                throw new ArgumentException($"Unknown importer: {_settings.ActiveImporter}");
        }
    }
}