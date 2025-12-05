using Core.DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly FragranceLogContext _context;

        public GroupsController(FragranceLogContext context)
        {
            _context = context;
        }

        [HttpGet("dictionary")]
        public async Task<ActionResult<List<GroupDictionaryItemDto>>> Dictionary()
        {
            var items = await _context.Groups
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => new GroupDictionaryItemDto
                {
                    Id = g.GroupId,
                    Name = g.Name
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
