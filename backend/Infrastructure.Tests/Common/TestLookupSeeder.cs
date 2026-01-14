using Infrastructure.Data;
using Core.Entities;

namespace Infrastructure.Tests.Common;

internal static class TestLookupSeeder
{
    public static void Seed(FragranceLogContext ctx)
    {
        ctx.Sillages.AddRange(
            new Sillage { SillageId = 1, Name = "Intimate" },
            new Sillage { SillageId = 2, Name = "Moderate" },
            new Sillage { SillageId = 3, Name = "Strong" },
            new Sillage { SillageId = 4, Name = "Enormous" }
        );

        ctx.Longevities.AddRange(
            new Longevity { LongevityId = 1, Name = "Very Weak" },
            new Longevity { LongevityId = 2, Name = "Weak" },
            new Longevity { LongevityId = 3, Name = "Moderate" },
            new Longevity { LongevityId = 4, Name = "Long Lasting" },
            new Longevity { LongevityId = 5, Name = "Eternal" }
        );

        ctx.Genders.AddRange(
            new Gender { GenderId = 1, Name = "Male" },
            new Gender { GenderId = 2, Name = "Female" },
            new Gender { GenderId = 3, Name = "Unisex" }
        );

        ctx.Seasons.AddRange(
            new Season { SeasonId = 1, Name = "Spring" },
            new Season { SeasonId = 2, Name = "Summer" },
            new Season { SeasonId = 3, Name = "Autumn" },
            new Season { SeasonId = 4, Name = "Winter" }
        );

        ctx.Daytimes.AddRange(
            new Daytime { DaytimeId = 1, Name = "Day" },
            new Daytime { DaytimeId = 2, Name = "Night" }
        );

        ctx.NoteTypes.AddRange(
            new NoteType { NoteTypeId = 1, Name = "Top" },
            new NoteType { NoteTypeId = 2, Name = "Middle" },
            new NoteType { NoteTypeId = 3, Name = "Base" }
        );

        ctx.Countries.AddRange(
            new Country { Code = "POL", Name = "Poland" },
            new Country { Code = "FRA", Name = "France" },
            new Country { Code = "ZZZ", Name = "Unknown" }
        );

        ctx.SaveChanges();
    }
}
