using Core.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/lists")]
public class PerfumeListsController : ControllerBase
{
    private readonly IPerfumeListService _service;

    public PerfumeListsController(IPerfumeListService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetLists()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var lists = await _service.GetUserListsAsync(userId ?? 0);
        return Ok(lists);
    }

    [HttpPost]
    public async Task<IActionResult> CreateList([FromBody] CreateListRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var list = await _service.CreateListAsync(userId ?? 0, request.Name);
        return Ok(list);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> RenameList(int id, [FromBody] RenameListRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _service.RenameListAsync(userId ?? 0, id, request.Name);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteList(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();


        await _service.DeleteListAsync(userId ?? 0, id);
        return NoContent();
    }

    [HttpGet("{id}/perfumes")]
    public async Task<IActionResult> GetListPerfumes(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var perfumes = await _service.GetListPerfumesAsync(userId ?? 0, id);
        return Ok(perfumes);
    }

    [HttpPost("{id}/perfumes/{perfumeId}")]
    public async Task<IActionResult> AddPerfume(int id, int perfumeId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _service.AddPerfumeToListAsync(userId ?? 0, id, perfumeId);
        return NoContent();
    }

    [HttpDelete("{id}/perfumes/{perfumeId}")]
    public async Task<IActionResult> RemovePerfume(int id, int perfumeId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _service.RemovePerfumeFromListAsync(userId ?? 0, id, perfumeId);
        return NoContent();
    }
}

public record CreateListRequest(string Name);
public record RenameListRequest(string Name);
