using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Common
{
    internal static class DbContextFactory
    {
        public static FragranceLogContext Create()
        {
            var options = new DbContextOptionsBuilder<FragranceLogContext>()
                .UseSqlite("Filename=:memory:")
                .Options;

            var context = new FragranceLogContext(options);

            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            return context;
        }
    }
}
