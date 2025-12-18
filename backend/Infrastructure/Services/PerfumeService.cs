using Core.DTOs;
using Core.Enums;
using Core.Exceptions;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class PerfumeService : IPerfumeService
    {
        private readonly FragranceLogContext _context;

        public PerfumeService(
                FragranceLogContext context
            )
        {
            _context = context;
        }

        public async Task<PerfumeSearchResponseDto> SearchAsync(
            PerfumeSearchRequestDto req,
            CancellationToken ct)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize is <= 0 or > 50 ? 25 : req.PageSize;

            var q = req.Query?.Trim().ToLower();

            var query =
                from p in _context.Perfumes.AsNoTracking()
                join r in _context.Reviews on p.PerfumeId equals r.PerfumeId into ratings
                join photo in _context.PerfumePhotos
                    on p.PerfumeId equals photo.PerfumeId into photos
                from photo in photos.DefaultIfEmpty()
                select new
                {
                    Perfume = p,
                    PhotoPath = photo.Path,
                    AvgRating = ratings.Any() ? ratings.Average(x => x.Rating) : 0,
                    RatingCount = ratings.Count()
                };

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(x =>
                    x.Perfume.Name.ToLower().Contains(q));
            }

            if (req.BrandId != null && req.BrandId != 0)
                query = query.Where(x => x.Perfume.BrandId == req.BrandId);

            if (!string.IsNullOrWhiteSpace(req.CountryCode))
                query = query.Where(x => x.Perfume.CountryCode == req.CountryCode);

            if (req.MinRating != null && req.MinRating != 0)
                query = query.Where(x => x.AvgRating >= req.MinRating);

            if (req.GroupIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    req.GroupIds.All(groupId =>
                        x.Perfume.Groups.Any(g => g.GroupId == groupId)));
            }

            if (req.Gender != null)
            {
                query = query.Where(x =>
                    x.Perfume.PerfumeGenderVotes
                        .GroupBy(v => v.GenderId)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault() == (int)req.Gender
                );
            }

            var totalCount = await query.CountAsync(ct);

            var result = await query
                .OrderByDescending(x =>
                    q != null && x.Perfume.Name.ToLower().StartsWith(q) ? 3 :
                    q != null && x.Perfume.Name.ToLower().Contains(q) ? 2 : 1)
                .ThenByDescending(x => x.AvgRating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PerfumeSearchResultDto
                {
                    PerfumeId = x.Perfume.PerfumeId,
                    Name = x.Perfume.Name,
                    Brand = x.Perfume.Brand.Name,
                    Rating = Math.Round(x.AvgRating, 2),
                    RatingCount = x.RatingCount,
                    CountryCode = x.Perfume.CountryCode,
                    ImageUrl = x.PhotoPath
                })
                .ToListAsync(ct);

            return new PerfumeSearchResponseDto
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = result
            };
        }

        public async Task<PerfumeDetailsDto> GetDetailsAsync(
            int perfumeId,
            int? userId,
            CancellationToken ct)
        {
            var dto = await _context.Perfumes
                .AsNoTracking()
                .Where(p => p.PerfumeId == perfumeId)
                .Select(p => new PerfumeDetailsDto
                {
                    PerfumeId = p.PerfumeId,
                    Name = p.Name,
                    Brand = p.Brand.Name,
                    ImageUrl = p.PerfumePhoto == null ? "" : p.PerfumePhoto.Path,

                    AvgRating = p.Reviews.Any()
                        ? Math.Round(p.Reviews.Average(r => (double)r.Rating), 2)
                        : 0d,

                    RatingCount = p.Reviews.Count(),
                    CommentCount = p.Reviews.Count(r => !string.IsNullOrWhiteSpace(r.Comment)),

                    Gender = p.PerfumeGenderVotes.Any()
                        ? (GenderEnum?)p.PerfumeGenderVotes
                            .GroupBy(v => v.GenderId)
                            .OrderByDescending(g => g.Count())
                            .Select(g => g.Key)
                            .First()
                        : null,

                    Season = p.PerfumeSeasonVotes.Any()
                        ? (SeasonEnum?)p.PerfumeSeasonVotes
                            .GroupBy(v => v.SeasonId)
                            .OrderByDescending(g => g.Count())
                            .Select(g => g.Key)
                            .First()
                        : null,

                    Daytime = p.PerfumeDaytimeVotes.Any()
                        ? (DaytimeEnum?)p.PerfumeDaytimeVotes
                            .GroupBy(v => v.DaytimeId)
                            .OrderByDescending(g => g.Count())
                            .Select(g => g.Key)
                            .First()
                        : null,

                    Longevity = p.PerfumeLongevityVotes.Any()
                        ? p.PerfumeLongevityVotes.Average(v => (double?)v.LongevityId)
                        : null,

                    Sillage = p.PerfumeSillageVotes.Any()
                        ? p.PerfumeSillageVotes.Average(v => (double?)v.SillageId)
                        : null,

                    Groups = p.Groups
                        .OrderBy(g => g.Name)
                        .Select(g => g.Name)
                        .ToList(),

                    NoteGroups = p.PerfumeNotes
                        .GroupBy(pn => pn.NoteTypeId)
                        .OrderBy(g => g.Key)
                        .Select(g => new PerfumeNoteGroupDto
                        {
                            Type = (NoteTypeEnum)g.Key,
                            Notes = g.Select(pn => new PerfumeNoteDto
                            {
                                NoteId = pn.NoteId,
                                Name = pn.Note.Name,
                                Type = (NoteTypeEnum)pn.NoteTypeId
                            }).ToList()
                        }).ToList(),

                    Reviews = p.Reviews
                        .Where(r => !string.IsNullOrWhiteSpace(r.Comment))
                        .OrderByDescending(r => r.ReviewDate)
                        .Take(20)
                        .Select(r => new ReviewDto
                        {
                            ReviewId = r.ReviewId,
                            Author = r.User.Username,
                            Rating = r.Rating,
                            Text = r.Comment,
                            CreatedAt = r.ReviewDate
                        })
                        .ToList(),

                    MyRating = userId == null
                        ? null
                        : p.Reviews
                            .Where(r => r.UserId == userId)
                            .Select(r => (int?)r.Rating)
                            .FirstOrDefault(),

                    MyReview = userId == null
                        ? null
                        : p.Reviews
                            .Where(r => r.UserId == userId)
                            .Select(r => r.Comment)
                            .FirstOrDefault(),

                    MyGenderVote = userId == null
                        ? null
                        : p.PerfumeGenderVotes
                            .Where(v => v.UserId == userId)
                            .Select(v => (GenderEnum?)v.GenderId)
                            .FirstOrDefault(),

                    MySeasonVote = userId == null
                        ? null
                        : p.PerfumeSeasonVotes
                            .Where(v => v.UserId == userId)
                            .Select(v => (SeasonEnum?)v.SeasonId)
                            .FirstOrDefault(),

                    MyDaytimeVote = userId == null
                        ? null
                        : p.PerfumeDaytimeVotes
                            .Where(v => v.UserId == userId)
                            .Select(v => (DaytimeEnum?)v.DaytimeId)
                            .FirstOrDefault(),

                    MyLongevityVote = userId == null
                        ? null
                        : p.PerfumeLongevityVotes
                            .Where(v => v.UserId == userId)
                            .Select(v => (LongevityEnum?)v.LongevityId)
                            .FirstOrDefault(),

                    MySillageVote = userId == null
                        ? null
                        : p.PerfumeSillageVotes
                            .Where(v => v.UserId == userId)
                            .Select(v => (SillageEnum?)v.SillageId)
                            .FirstOrDefault()
                })
                .SingleOrDefaultAsync(ct);

            if (dto == null)
                throw new NotFoundException("Perfume not found.");

            return dto;
        }
    }
}
