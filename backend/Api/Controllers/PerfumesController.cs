using Core.DTOs;
using Core.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PerfumesController : ControllerBase
    {
        private readonly IPerfumeService _perfumeService;

        public PerfumesController(IPerfumeService perfumeService)
        {
            _perfumeService = perfumeService;
        }

        [HttpPost("search")]
        public async Task<ActionResult<PerfumeSearchResponseDto>> Search(
            [FromBody] PerfumeSearchRequestDto req,
            CancellationToken ct)
        {
            var result = await _perfumeService.SearchAsync(req, ct);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PerfumeDetailsDto>> GetById(int id, CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _perfumeService.GetDetailsAsync(id, userId, ct);
            return Ok(result);
        }
    }
}
