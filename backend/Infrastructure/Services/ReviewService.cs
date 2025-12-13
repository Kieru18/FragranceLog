using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class ReviewService : IReviewService
{
    private readonly FragranceLogContext _context;

    public ReviewService(FragranceLogContext context)
    {
        _context = context;
    }

    public async Task CreateOrUpdateAsync(int userId, CreateReviewDto dto)
    {
        var existing = await _context.Reviews
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.PerfumeId == dto.PerfumeId);

        if (existing == null)
        {
            var review = new Review
            {
                UserId = userId,
                PerfumeId = dto.PerfumeId,
                Rating = dto.Rating,
                Comment = string.IsNullOrWhiteSpace(dto.Text) ? null : dto.Text,
                ReviewDate = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
        }
        else
        {
            existing.Rating = dto.Rating;
            existing.Comment = string.IsNullOrWhiteSpace(dto.Text) ? null : dto.Text;
            existing.ReviewDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}

