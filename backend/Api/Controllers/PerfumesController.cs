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
        public async Task<ActionResult<List<PerfumeSearchResultDto>>> Search(
        [FromQuery] PerfumeSearchRequestDto req)
        {
            var q = req.Query.Trim().ToLower();

            var query = _context.Perfumes
                .AsNoTracking()
                .Where(p =>
                    string.IsNullOrEmpty(q) ||
                    p.Name.ToLower().Contains(q));

            if (req.BrandId != null)
                query = query.Where(p => p.BrandId == req.BrandId);

            if (!string.IsNullOrEmpty(req.CountryCode))
                query = query.Where(p => p.CountryCode == req.CountryCode);

            var results = await query
                .Select(p => new
                {
                    p,
                    Rating = p.Reviews.Any()
                        ? p.Reviews.Average(r => r.Rating)
                        : 0,
                    RatingCount = p.Reviews.Count
                })
                .OrderByDescending(x =>
                    x.p.Name.ToLower().StartsWith(q) ? 3 :
                    x.p.Name.ToLower().Contains(q) ? 2 : 1)
                .ThenByDescending(x => x.Rating)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .Select(x => new PerfumeSearchResultDto
                {
                    PerfumeId = x.p.PerfumeId,
                    Name = x.p.Name,
                    Brand = x.p.Brand.Name,
                    Country = x.p.CountryCode,
                    Rating = Math.Round(x.Rating, 2),
                    RatingCount = x.RatingCount
                })
                .ToListAsync();

            return results;
        }
    }
}
