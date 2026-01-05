using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public sealed class PerfumeAnalyticsService : IPerfumeAnalyticsService
{
    private readonly FragranceLogContext _context;

    public PerfumeAnalyticsService(FragranceLogContext context)
    {
        _context = context;
    }

    public async Task<PerfumeOfTheDayDto?> GetPerfumeOfTheDayAsync()
    {
        var now = DateTime.UtcNow;

        return
            await TryWindow(now.AddHours(-24), PerfumeHighlightType.Day, "Most loved today")
         ?? await TryWindow(now.AddHours(-72), PerfumeHighlightType.Recent, "Trending recently")
         ?? await TryWindow(now.AddDays(-7), PerfumeHighlightType.Week, "Perfume of the Week")
         ?? await GetCommunityFavorite()
         ?? await GetSpotlightPerfume();
    }

    public async Task<IReadOnlyList<HomeRecentReviewDto>> GetRecentReviewsAsync(
        int take,
        CancellationToken ct)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => !string.IsNullOrWhiteSpace(r.Comment))
            .OrderByDescending(r => r.ReviewDate)
            .Take(take)
            .Select(r => new HomeRecentReviewDto
            {
                ReviewId = r.ReviewId,
                PerfumeId = r.PerfumeId,

                PerfumeName = r.Perfume.Name,
                Brand = r.Perfume.Brand.Name,
                PerfumeImageUrl = r.Perfume.PerfumePhoto == null
                    ? null
                    : r.Perfume.PerfumePhoto.Path,

                Author = r.User.Username,
                Rating = r.Rating,
                Comment = r.Comment!,
                CreatedAt = r.ReviewDate
            })
            .ToListAsync(ct);
    }

    public async Task<HomeStatsDto> GetStatsAsync(CancellationToken ct)
    {
        var totalPerfumes = await _context.Perfumes.CountAsync(ct);
        var totalReviews = await _context.Reviews.CountAsync(ct);
        var totalUsers = await _context.Users.CountAsync(ct);

        return new HomeStatsDto
        {
            Perfumes = totalPerfumes,
            Reviews = totalReviews,
            Users = totalUsers
        };
    }

    public async Task<HomeInsightDto?> GetHomeInsightAsync(CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddHours(-24);

        var stats = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewDate >= since)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Avg = g.Average(r => (double)r.Rating),
                Count = g.Count()
            })
            .SingleOrDefaultAsync(ct);

        if (stats == null || stats.Count == 0)
            return null;

        return new HomeInsightDto
        {
            Title = "Community mood",
            Subtitle = $"Average rating {stats.Avg:F2} based on {stats.Count} recent reviews",
            Icon = "chart-line"
        };
    }


    private async Task<PerfumeOfTheDayDto?> TryWindow(
        DateTime from,
        PerfumeHighlightType type,
        string reason)
    {
        var candidate =
            await _context.Reviews
                .Where(r => r.ReviewDate >= from)
                .GroupBy(r => r.PerfumeId)
                .Where(g => g.Count() >= 2)
                .Select(g => new PerfumeScoreRow
                {
                    PerfumeId = g.Key,
                    Score =
                        g.Count(x => x.Rating == 5) * 5.0 +
                        g.Count(x => x.Rating == 4) * 2.5 +
                        g.Count(x => x.Rating == 3) * 1.0 -
                        g.Count(x => x.Rating == 1) * 2.0,
                    AvgRating = g.Average(x => x.Rating),
                    RatingCount = g.Count()
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.AvgRating)
                .FirstOrDefaultAsync();

        if (candidate == null)
            return null;

        return await MapToDto(candidate, type, reason);
    }


    private async Task<PerfumeOfTheDayDto?> GetCommunityFavorite()
    {
        var candidate =
            await _context.Reviews
                .GroupBy(r => r.PerfumeId)
                .Where(g => g.Count() >= 5)
                .Select(g => new PerfumeScoreRow
                {
                    PerfumeId = g.Key,
                    AvgRating = g.Average(x => x.Rating),
                    RatingCount = g.Count(),
                    Score = g.Average(x => x.Rating) * Math.Log10(1 + g.Count())
                })
                .OrderByDescending(x => x.Score)
                .FirstOrDefaultAsync();

        if (candidate == null)
            return null;

        return await MapToDto(
            candidate,
            PerfumeHighlightType.Favorite,
            "Community favorite"
        );
    }

    private async Task<PerfumeOfTheDayDto> MapToDto(
        PerfumeScoreRow data,
        PerfumeHighlightType type,
        string reason)
    {
        var dto =
            await _context.Perfumes
                .AsNoTracking()
                .Where(p => p.PerfumeId == data.PerfumeId)
                .Select(p => new PerfumeOfTheDayDto
                {
                    PerfumeId = p.PerfumeId,
                    Name = p.Name,
                    Brand = p.Brand.Name,
                    ImageUrl = p.PerfumePhoto == null
                        ? ""
                        : p.PerfumePhoto.Path,

                    AvgRating = data.AvgRating,
                    RatingCount = data.RatingCount,
                    Score = data.Score,
                    Type = type,
                    Reason = reason
                })
                .SingleAsync();

        return dto;
    }

    private async Task<PerfumeOfTheDayDto?> GetSpotlightPerfume()
    {
        var dto =
            await _context.Perfumes
                .AsNoTracking()
                .Where(p => p.PerfumePhoto != null)
                .OrderByDescending(p => p.PerfumeId)
                .Select(p => new PerfumeOfTheDayDto
                {
                    PerfumeId = p.PerfumeId,
                    Name = p.Name,
                    Brand = p.Brand.Name,
                    ImageUrl = p.PerfumePhoto == null
                        ? ""
                        : p.PerfumePhoto.Path,

                    AvgRating = 0,
                    RatingCount = 0,
                    Score = 0,

                    Type = PerfumeHighlightType.Spotlight,
                    Reason = "Featured perfume"
                })
                .FirstOrDefaultAsync();

        return dto;
    }
}
