using Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Common;

internal static class DbContextFactory
{
    public static (FragranceLogContext ctx, SqliteConnection conn) Create()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        connection.CreateFunction("getdate", () => DateTime.UtcNow);
        connection.CreateFunction("GETDATE", () => DateTime.UtcNow);

        var options = new DbContextOptionsBuilder<FragranceLogContext>()
            .UseSqlite(connection)
            .Options;

        var context = new FragranceLogContext(options);
        context.Database.EnsureCreated();

        TestLookupSeeder.Seed(context);

        return (context, connection);
    }
}
