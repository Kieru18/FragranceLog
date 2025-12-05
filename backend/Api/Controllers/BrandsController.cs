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
    public class BrandsController : ControllerBase
    {
        private readonly FragranceLogContext _context;

        public BrandsController(FragranceLogContext context)
        {
            _context = context;
        }

        [HttpGet("dictionary")]
        public async Task<ActionResult<List<BrandDictionaryItemDto>>> Dictionary()
        {
            var items = await _context.Brands
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => new BrandDictionaryItemDto
                {
                    Id = g.BrandId,
                    Name = g.Name
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
