using CsvHelper.Expressions;
using DataImporter.Configuration;
using DataImporter.Importers;
using DataImporter.Services;
using DataImporter.Tools;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace DataImporter;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var connectionName = context.Configuration["Import:ConnectionName"] ?? "";

                var connectionString = context.Configuration.GetConnectionString(connectionName)
                                      ?? throw new InvalidOperationException($"Connection string {connectionName} not found.");

                var importSection = context.Configuration.GetSection("Import");
                var importOptions = importSection.Get<ImportOptions>()
                                    ?? throw new InvalidOperationException("Import section missing.");

                services.Configure<ImportOptions>(importSection);

                services.AddDbContext<FragranceLogContext>(options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        sql =>
                        {
                            sql.CommandTimeout(180);
                            sql.EnableRetryOnFailure(
                                maxRetryCount: 2,
                                maxRetryDelay: TimeSpan.FromSeconds(100),
                                errorNumbersToAdd: null);
                        });
                });

                services.AddSingleton<NameNormalizer>();
                services.AddSingleton<JsonlReader>();
                services.AddSingleton<PerfumeMatcher>();
                services.AddSingleton<ReportWriter>();

                services.AddSingleton<CsvReaderService>();
                services.AddSingleton<CountryAliasResolver>();
                services.AddSingleton<SyntheticDataService>();
                services.AddTransient<KaggleImporter>();

                services.AddSingleton<BrandAliasResolver>();
                services.AddSingleton<ImageFileService>();
                services.AddSingleton<ImageDbWriterService>();
                services.AddTransient<ImageImporter>();

                services.AddTransient<BrandComparisonTool>();

                switch (importOptions.Mode.ToLower())
                {
                    case "kaggle":
                        services.AddTransient<IImporter, KaggleImporter>();
                        break;
                    case "images":
                        services.AddTransient<IImporter, ImageImporter>();
                        break;
                    case "brandcomparison":
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown import mode: {importOptions.Mode}");
                }
            })

            .Build();

        using (host)
        {
            var cfg = host.Services.GetRequiredService<IOptions<ImportOptions>>().Value;

            if (cfg.Mode.Equals("brandcomparison", StringComparison.OrdinalIgnoreCase))
            {
                var tool = host.Services.GetRequiredService<BrandComparisonTool>();
                await tool.RunAsync(cfg.Images.JsonlPath!);
                return;
            }

            var importer = host.Services.GetRequiredService<IImporter>();
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

            await importer.RunAsync(lifetime.ApplicationStopping);
        }
    }
}
