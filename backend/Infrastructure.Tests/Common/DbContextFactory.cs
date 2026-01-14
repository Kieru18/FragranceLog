using Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Common
{
    internal static class DbContextFactory
    {
        public static FragranceLogContext Create()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            connection.CreateFunction(
                "getdate",
                () => DateTime.UtcNow
            );

            var options = new DbContextOptionsBuilder<FragranceLogContext>()
                .UseSqlite(connection)
                .Options;

            var context = new FragranceLogContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
