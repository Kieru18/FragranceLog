using Core.DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PerfumesController : ControllerBase
    {
        private readonly FragranceLogContext _context;

        public PerfumesController(FragranceLogContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<PerfumeSearchResultDto>>> Search(
        [FromQuery] PerfumeSearchRequestDto req,
        CancellationToken ct)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize is <= 0 or > 50 ? 25 : req.PageSize;

            var q = req.Query?.Trim().ToLower();

            var query =
                from p in _context.Perfumes.AsNoTracking()
                join r in _context.Reviews on p.PerfumeId equals r.PerfumeId into ratings
                select new
                {
                    Perfume = p,
                    AvgRating = ratings.Any() ? ratings.Average(x => x.Rating) : 0,
                    RatingCount = ratings.Count()
                };

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(x =>
                    x.Perfume.Name.ToLower().Contains(q));
            }

            if (req.BrandId != null)
                query = query.Where(x => x.Perfume.BrandId == req.BrandId);

            if (!string.IsNullOrWhiteSpace(req.CountryCode))
                query = query.Where(x => x.Perfume.CountryCode == req.CountryCode);

            if (req.MinRating != null)
                query = query.Where(x => x.AvgRating >= req.MinRating);

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
                    CountryCode = x.Perfume.CountryCode
                })
                .ToListAsync(ct);

            return Ok(result);
        }
    }
}
