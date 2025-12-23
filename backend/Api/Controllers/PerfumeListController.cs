using Core.DTOs;
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
    private readonly IPerfumeListService _perfumeListService;
    private readonly ISharedListService _sharedListService;

    public PerfumeListsController(IPerfumeListService perfumeListService, ISharedListService sharedListService)
    {
        _perfumeListService = perfumeListService;
        _sharedListService = sharedListService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLists()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var lists = await _perfumeListService.GetUserListsAsync(userId ?? 0);
        return Ok(lists);
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetListsOverview()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _perfumeListService.GetListsOverviewAsync(userId.Value);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateList([FromBody] CreateListRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var list = await _perfumeListService.CreateListAsync(userId ?? 0, request.Name);
        return Ok(list);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> RenameList(int id, [FromBody] RenameListRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _perfumeListService.RenameListAsync(userId ?? 0, id, request.Name);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteList(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();


        await _perfumeListService.DeleteListAsync(userId ?? 0, id);
        return NoContent();
    }

    [HttpGet("{id}/perfumes")]
    public async Task<IActionResult> GetListPerfumes(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var perfumes = await _perfumeListService.GetListPerfumesAsync(userId ?? 0, id);
        return Ok(perfumes);
    }

    [HttpPost("{id}/perfumes/{perfumeId}")]
    public async Task<IActionResult> AddPerfume(int id, int perfumeId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _perfumeListService.AddPerfumeToListAsync(userId ?? 0, id, perfumeId);
        return NoContent();
    }

    [HttpDelete("{id}/perfumes/{perfumeId}")]
    public async Task<IActionResult> RemovePerfume(int id, int perfumeId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _perfumeListService.RemovePerfumeFromListAsync(userId ?? 0, id, perfumeId);
        return NoContent();
    }

    [HttpGet("for-perfume/{perfumeId}")]
    public async Task<IActionResult> GetListsForPerfume(int perfumeId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _perfumeListService.GetListsForPerfumeAsync(
            userId.Value,
            perfumeId
        );

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetList(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var list = await _perfumeListService.GetListAsync(userId ?? 0, id);
        return Ok(list);
    }

    [HttpPost("{id}/share")]
    public async Task<ActionResult<SharedListDto>> Share(int id)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _sharedListService.ShareAsync(userId.Value, id);
        return Ok(result);
    }
}

public record CreateListRequest(string Name);
public record RenameListRequest(string Name);
