using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public sealed class PerfumeVoteService : IPerfumeVoteService
{
    private readonly FragranceLogContext _context;

    public PerfumeVoteService(FragranceLogContext context)
    {
        _context = context;
    }

    public async Task SetGenderVoteAsync(int perfumeId, int userId, GenderEnum? gender)
    {
        var existing = await _context.PerfumeGenderVotes
            .FirstOrDefaultAsync(v => v.PerfumeId == perfumeId && v.UserId == userId);

        if (gender == null)
        {
            if (existing != null)
            {
                _context.PerfumeGenderVotes.Remove(existing);
                await _context.SaveChangesAsync();
            }
            return;
        }

        if (existing == null)
        {
            _context.PerfumeGenderVotes.Add(new PerfumeGenderVote
            {
                PerfumeId = perfumeId,
                UserId = userId,
                GenderId = (int)gender
            });
        }
        else
        {
            existing.GenderId = (int)gender;
            existing.VoteDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetSillageVoteAsync(int perfumeId, int userId, SillageEnum? sillage)
    {
        var existing = await _context.PerfumeSillageVotes
            .FirstOrDefaultAsync(v => v.PerfumeId == perfumeId && v.UserId == userId);

        if (sillage == null)
        {
            if (existing != null)
            {
                _context.PerfumeSillageVotes.Remove(existing);
                await _context.SaveChangesAsync();
            }
            return;
        }

        if (existing == null)
        {
            _context.PerfumeSillageVotes.Add(new PerfumeSillageVote
            {
                PerfumeId = perfumeId,
                UserId = userId,
                SillageId = (int)sillage
            });
        }
        else
        {
            existing.SillageId = (int)sillage;
            existing.VoteDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetLongevityVoteAsync(int perfumeId, int userId, LongevityEnum? longevity)
    {
        var existing = await _context.PerfumeLongevityVotes
            .FirstOrDefaultAsync(v => v.PerfumeId == perfumeId && v.UserId == userId);

        if (longevity == null)
        {
            if (existing != null)
            {
                _context.PerfumeLongevityVotes.Remove(existing);
                await _context.SaveChangesAsync();
            }
            return;
        }

        if (existing == null)
        {
            _context.PerfumeLongevityVotes.Add(new PerfumeLongevityVote
            {
                PerfumeId = perfumeId,
                UserId = userId,
                LongevityId = (int)longevity
            });
        }
        else
        {
            existing.LongevityId = (int)longevity;
            existing.VoteDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetSeasonVoteAsync(int perfumeId, int userId, SeasonEnum? season)
    {
        var existing = await _context.PerfumeSeasonVotes
            .FirstOrDefaultAsync(v => v.PerfumeId == perfumeId && v.UserId == userId);

        if (season == null)
        {
            if (existing != null)
            {
                _context.PerfumeSeasonVotes.Remove(existing);
                await _context.SaveChangesAsync();
            }
            return;
        }

        if (existing == null)
        {
            _context.PerfumeSeasonVotes.Add(new PerfumeSeasonVote
            {
                PerfumeId = perfumeId,
                UserId = userId,
                SeasonId = (int)season
            });
        }
        else
        {
            existing.SeasonId = (int)season;
            existing.VoteDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetDaytimeVoteAsync(int perfumeId, int userId, DaytimeEnum? daytime)
    {
        var existing = await _context.PerfumeDaytimeVotes
            .FirstOrDefaultAsync(v => v.PerfumeId == perfumeId && v.UserId == userId);

        if (daytime == null)
        {
            if (existing != null)
            {
                _context.PerfumeDaytimeVotes.Remove(existing);
                await _context.SaveChangesAsync();
            }
            return;
        }

        if (existing == null)
        {
            _context.PerfumeDaytimeVotes.Add(new PerfumeDaytimeVote
            {
                PerfumeId = perfumeId,
                UserId = userId,
                DaytimeId = (int)daytime
            });
        }
        else
        {
            existing.DaytimeId = (int)daytime;
            existing.VoteDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
