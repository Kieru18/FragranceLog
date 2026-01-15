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
            var perfume = await _context.Perfumes
                .AsNoTracking()
                .Include(p => p.Brand)
                .Include(p => p.PerfumePhoto)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .Include(p => p.PerfumeGenderVotes)
                .Include(p => p.PerfumeSeasonVotes)
                .Include(p => p.PerfumeDaytimeVotes)
                .Include(p => p.PerfumeLongevityVotes)
                .Include(p => p.PerfumeSillageVotes)
                .Include(p => p.Groups)
                .Include(p => p.PerfumeNotes).ThenInclude(pn => pn.Note)
                .SingleOrDefaultAsync(p => p.PerfumeId == perfumeId, ct);

            if (perfume == null)
                throw new NotFoundException("Perfume not found.");


            var dto = new PerfumeDetailsDto
            {
                PerfumeId = perfume.PerfumeId,
                Name = perfume.Name,
                Brand = perfume.Brand.Name,
                ImageUrl = perfume.PerfumePhoto?.Path ?? "",

                AvgRating = perfume.Reviews.Any()
                    ? Math.Round(perfume.Reviews.Average(r => (double)r.Rating), 2)
                    : 0d,

                RatingCount = perfume.Reviews.Count,
                CommentCount = perfume.Reviews.Count(r => !string.IsNullOrWhiteSpace(r.Comment)),

                Gender = perfume.PerfumeGenderVotes.Any()
                    ? (GenderEnum?)perfume.PerfumeGenderVotes
                        .GroupBy(v => v.GenderId)
                        .OrderByDescending(g => g.Count())
                        .First().Key
                    : null,

                Season = perfume.PerfumeSeasonVotes.Any()
                    ? (SeasonEnum?)perfume.PerfumeSeasonVotes
                        .GroupBy(v => v.SeasonId)
                        .OrderByDescending(g => g.Count())
                        .First().Key
                    : null,

                Daytime = perfume.PerfumeDaytimeVotes.Any()
                    ? (DaytimeEnum?)perfume.PerfumeDaytimeVotes
                        .GroupBy(v => v.DaytimeId)
                        .OrderByDescending(g => g.Count())
                        .First().Key
                    : null,

                Longevity = perfume.PerfumeLongevityVotes.Any()
                    ? perfume.PerfumeLongevityVotes.Average(v => (double?)v.LongevityId)
                    : null,

                Sillage = perfume.PerfumeSillageVotes.Any()
                    ? perfume.PerfumeSillageVotes.Average(v => (double?)v.SillageId)
                    : null,

                Groups = perfume.Groups
                    .OrderBy(g => g.Name)
                    .Select(g => g.Name)
                    .ToList(),

                NoteGroups = perfume.PerfumeNotes
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

                Reviews = perfume.Reviews
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
                    : perfume.Reviews
                        .Where(r => r.UserId == userId)
                        .Select(r => (int?)r.Rating)
                        .FirstOrDefault(),

                MyReview = userId == null
                    ? null
                    : perfume.Reviews
                        .Where(r => r.UserId == userId)
                        .Select(r => r.Comment)
                        .FirstOrDefault(),

                MyGenderVote = userId == null
                    ? null
                    : perfume.PerfumeGenderVotes
                        .Where(v => v.UserId == userId)
                        .Select(v => (GenderEnum?)v.GenderId)
                        .FirstOrDefault(),

                MySeasonVote = userId == null
                    ? null
                    : perfume.PerfumeSeasonVotes
                        .Where(v => v.UserId == userId)
                        .Select(v => (SeasonEnum?)v.SeasonId)
                        .FirstOrDefault(),

                MyDaytimeVote = userId == null
                    ? null
                    : perfume.PerfumeDaytimeVotes
                        .Where(v => v.UserId == userId)
                        .Select(v => (DaytimeEnum?)v.DaytimeId)
                        .FirstOrDefault(),

                MyLongevityVote = userId == null
                    ? null
                    : perfume.PerfumeLongevityVotes
                        .Where(v => v.UserId == userId)
                        .Select(v => (LongevityEnum?)v.LongevityId)
                        .FirstOrDefault(),

                MySillageVote = userId == null
                ? null
                : perfume.PerfumeSillageVotes
                    .Where(v => v.UserId == userId)
                    .Select(v => (SillageEnum?)v.SillageId)
                    .FirstOrDefault()
            };

            return dto;
        }
    }
}
