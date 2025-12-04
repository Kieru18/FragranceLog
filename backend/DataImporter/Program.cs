using System.Threading.Tasks;
using DataImporter.Configuration;
using DataImporter.Importers;
using DataImporter.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                var connectionString = context.Configuration.GetConnectionString("FragranceLog")
                                      ?? throw new InvalidOperationException("Connection string 'FragranceLog' not found.");

                services.Configure<ImportOptions>(context.Configuration.GetSection("Import"));

                services.AddDbContext<FragranceLogContext>(options =>
                    options.UseSqlServer(connectionString));

                services.AddSingleton<CsvReaderService>();
                services.AddSingleton<CountryAliasResolver>();
                services.AddSingleton<SyntheticDataService>();

                services.AddTransient<KaggleImporter>();
            })
            .Build();

        using (host)
        {
            var importer = host.Services.GetRequiredService<KaggleImporter>();
            await importer.RunAsync();
        }
    }
}
