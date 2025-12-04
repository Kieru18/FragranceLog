using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Entities;
using DataImporter.Configuration;
using DataImporter.Models;
using DataImporter.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DataImporter.Importers;

public class KaggleImporter
{
    private readonly FragranceLogContext _db;
    private readonly CsvReaderService _csvReader;
    private readonly CountryAliasResolver _countryResolver;
    private readonly SyntheticDataService _synthetic;
    private readonly ImportOptions _options;

    public KaggleImporter(
        FragranceLogContext db,
        CsvReaderService csvReader,
        CountryAliasResolver countryResolver,
        SyntheticDataService synthetic,
        IOptions<ImportOptions> options)
    {
        _db = db;
        _csvReader = csvReader;
        _countryResolver = countryResolver;
        _synthetic = synthetic;
        _options = options.Value;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Reading CSV...");
        var rows = _csvReader.ReadRows(_options.CsvPath)
            .Where(r => !string.IsNullOrWhiteSpace(r.PerfumeName)
                     && !string.IsNullOrWhiteSpace(r.BrandName))
            .ToList();

        Console.WriteLine($"Loaded {rows.Count} rows.");

        var unknownCompany = await EnsureUnknownCompanyAsync(cancellationToken);
        var importerUser = await EnsureImporterUserAsync(cancellationToken);

        var countriesByName = await _db.Countries
            .ToDictionaryAsync(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var brandsByName = await _db.Brands
            .Include(b => b.Company)
            .ToDictionaryAsync(b => b.Name, b => b, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var notesByName = await _db.Notes
            .ToDictionaryAsync(n => n.Name, n => n, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var groupsByName = await _db.Groups
            .ToDictionaryAsync(g => g.Name, g => g, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var noteTypesByName = await _db.NoteTypes
            .ToDictionaryAsync(nt => nt.Name, nt => nt, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var gendersByName = await _db.Genders
            .ToDictionaryAsync(g => g.Name, g => g, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var existingPerfumes = await _db.Perfumes
            .Include(p => p.Brand)
            .ToListAsync(cancellationToken);

        var perfumeKeySet = new HashSet<string>(
            existingPerfumes.Select(p => $"{p.Brand.Name}||{p.Name}"),
            StringComparer.OrdinalIgnoreCase);

        Console.WriteLine("Importing perfumes...");

        var processed = 0;

        using var transaction = _db.Database.BeginTransaction();

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            processed++;
            if (processed % 1000 == 0)
            {
                Console.WriteLine($"Processed {processed}/{rows.Count}...");
            }

            var countryEntity = _countryResolver.Resolve(row.Country, countriesByName);
            if (countryEntity == null)
            {
                transaction.Rollback();
                throw new Exception($"Country '{row.Country}' could not be resolved.");
            }

            var brand = EnsureBrand(row.BrandName, unknownCompany, brandsByName);

            var perfumeNameTrimmed = row.PerfumeName.Trim();
            var perfumeKey = $"{brand.Name}||{perfumeNameTrimmed}";
            if (perfumeKeySet.Contains(perfumeKey))
            {
                continue;
            }

            var perfume = new Perfume
            {
                Name = perfumeNameTrimmed,
                BrandId = brand.BrandId,
                CountryCode = countryEntity.Code,
                LaunchYear = ParseYear(row.YearRaw),
                Description = null
            };

            _db.Perfumes.Add(perfume);
            await _db.SaveChangesAsync(cancellationToken);

            perfumeKeySet.Add(perfumeKey);

            AddNotes(perfume, row.TopNotesRaw, "Top", notesByName, noteTypesByName);
            AddNotes(perfume, row.MiddleNotesRaw, "Middle", notesByName, noteTypesByName);
            AddNotes(perfume, row.BaseNotesRaw, "Base", notesByName, noteTypesByName);

            foreach (var accord in row.GetAccords())
            {
                if (!groupsByName.TryGetValue(accord, out var group))
                {
                    group = new Group { Name = accord };
                    _db.Groups.Add(group);
                    await _db.SaveChangesAsync(cancellationToken);
                    groupsByName[accord] = group;
                }

                perfume.Groups ??= new List<Group>();
                if (!perfume.Groups.Contains(group))
                {
                    perfume.Groups.Add(group);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            var ratingCount = ParseInt(row.RatingCountRaw);
            var ratingValue = ParseDouble(row.RatingValueRaw);

            if (ratingCount > 0 && ratingValue > 0)
            {
                await GenerateReviewsAsync(perfume, ratingValue, ratingCount, importerUser, cancellationToken);
                await GenerateGenderVoteAsync(perfume, row.Gender, importerUser, gendersByName, cancellationToken);
            }
        }

        transaction.Commit();
        Console.WriteLine($"Import finished. Processed {processed} rows.");
    }

    private async Task<Company> EnsureUnknownCompanyAsync(CancellationToken ct)
    {
        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.Name == "Unknown", ct);

        if (company == null)
        {
            throw new ArgumentNullException("Unknown Company does not exist.");
        }

        return company;
    }

    private async Task<User> EnsureImporterUserAsync(CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == "DataImporter", ct);

        if (user == null)
        {
            user = new User
            {
                Username = "DataImporter",
                Email = "importer@fragrance.log",
                Password = _options.ImporterUser.Password
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
        }

        return user;
    }

    private Brand EnsureBrand(
        string rawBrandName,
        Company unknownCompany,
        Dictionary<string, Brand> brandsByName)
    {
        var name = rawBrandName.Trim();

        if (!brandsByName.TryGetValue(name, out var brand))
        {
            brand = new Brand
            {
                Name = name,
                CompanyId = unknownCompany.CompanyId
            };

            _db.Brands.Add(brand);
            _db.SaveChanges();
            brandsByName[name] = brand;
        }

        return brand;
    }

    private int? ParseYear(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        if (int.TryParse(raw.Trim(), out var y))
        {
            if (y >= 1800 && y <= DateTime.Now.Year)
                return y;
        }

        return null;
    }

    private int ParseInt(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return 0;

        var trimmed = raw.Trim().Replace(" ", "");
        return int.TryParse(trimmed, out var v) ? v : 0;
    }

    private double ParseDouble(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return 0;

        var s = raw.Trim().Replace(" ", "").Replace(',', '.');

        return double.TryParse(
            s,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v)
            ? v
            : 0;
    }

    private void AddNotes(
        Perfume perfume,
        string? notesRaw,
        string noteTypeName,
        Dictionary<string, Note> notesByName,
        Dictionary<string, NoteType> noteTypesByName)
    {
        if (string.IsNullOrWhiteSpace(notesRaw))
            return;

        if (!noteTypesByName.TryGetValue(noteTypeName, out var noteType))
            throw new InvalidOperationException($"NoteType '{noteTypeName}' not found. Seed NoteTypes with Top/Middle/Base.");

        var split = notesRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var n in split)
        {
            if (!notesByName.TryGetValue(n, out var note))
            {
                note = new Note
                {
                    Name = n
                };
                _db.Notes.Add(note);
                _db.SaveChanges();
                notesByName[n] = note;
            }

            var perfumeNote = new PerfumeNote
            {
                PerfumeId = perfume.PerfumeId,
                NoteId = note.NoteId,
                NoteTypeId = noteType.NoteTypeId
            };

            _db.PerfumeNotes.Add(perfumeNote);
        }
    }

    private async Task GenerateReviewsAsync(
        Perfume perfume,
        double ratingValue,
        int ratingCount,
        User importerUser,
        CancellationToken ct)
    {
        var ratings = _synthetic.BuildRatings(ratingValue, ratingCount);
        if (ratings.Count == 0)
            return;

        foreach (var r in ratings)
        {
            var review = new Review
            {
                UserId = importerUser.UserId,
                PerfumeId = perfume.PerfumeId,
                Rating = r,
                Comment = null,
                ReviewDate = DateTime.UtcNow
            };

            _db.Reviews.Add(review);
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task GenerateGenderVoteAsync(
        Perfume perfume,
        string? rawGender,
        User importerUser,
        Dictionary<string, Gender> gendersByName,
        CancellationToken ct)
    {
        var genderName = _synthetic.MapGenderName(rawGender);
        if (!gendersByName.TryGetValue(genderName, out var gender))
        {
            Console.WriteLine($"[WARN] Gender '{genderName}' not found in Genders table. Skipping gender vote.");
            return;
        }

        // PK (PerfumeId, UserId) ⇒ only ONE vote allowed
        var existingVote = await _db.PerfumeGenderVotes
            .FindAsync(new object[] { perfume.PerfumeId, importerUser.UserId }, ct);

        if (existingVote != null)
            return;

        var vote = new PerfumeGenderVote
        {
            PerfumeId = perfume.PerfumeId,
            UserId = importerUser.UserId,
            GenderId = gender.GenderId,
            VoteDate = DateTime.UtcNow
        };

        _db.PerfumeGenderVotes.Add(vote);
        await _db.SaveChangesAsync(ct);
    }
}
