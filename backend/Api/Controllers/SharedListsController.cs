using Core.DTOs;
using Core.Entities;
using Core.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/shared-lists")]
    public class SharedListsController : ControllerBase
    {
        private readonly ISharedListService _service;

        public SharedListsController(ISharedListService service)
        {
            _service = service;
        }

        [HttpGet("{token}")]
        public async Task<ActionResult<SharedListPreviewDto>> GetPreview(Guid token)
        {
            var result = await _service.GetPreviewAsync(token);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [Authorize]
        [HttpPost("{token}/import")]
        public async Task<IActionResult> Import(Guid token)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var newListId = await _service.ImportAsync(userId.Value, token);
            return Ok(new { listId = newListId });
        }
    }
}
