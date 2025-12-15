using Core.DTOs;
using Core.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/perfumes/{perfumeId:int}/votes")]
public sealed class PerfumeVotesController : ControllerBase
{
    private readonly IPerfumeVoteService _voteService;

    public PerfumeVotesController(IPerfumeVoteService voteService)
    {
        _voteService = voteService;
    }


    [HttpPut("gender")]
    public async Task<IActionResult> SetGenderVote(
        int perfumeId,
        [FromBody] SetGenderVoteDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _voteService.SetGenderVoteAsync(perfumeId, userId ?? 0, dto.Gender);
        return NoContent();
    }

    [HttpPut("sillage")]
    public async Task<IActionResult> SetSillageVote(
        int perfumeId,
        [FromBody] SetSillageVoteDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null)

            return Unauthorized();
        await _voteService.SetSillageVoteAsync(perfumeId, userId ?? 0, dto.Sillage);
        return NoContent();
    }

    [HttpPut("longevity")]
    public async Task<IActionResult> SetLongevityVote(
        int perfumeId,
        [FromBody] SetLongevityVoteDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _voteService.SetLongevityVoteAsync(perfumeId, userId ?? 0, dto.Longevity);
        return NoContent();
    }

    [HttpPut("season")]
    public async Task<IActionResult> SetSeasonVote(
        int perfumeId,
        [FromBody] SetSeasonVoteDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _voteService.SetSeasonVoteAsync(perfumeId, userId ?? 0, dto.Season);
        return NoContent();
    }

    [HttpPut("daytime")]
    public async Task<IActionResult> SetDaytimeVote(
        int perfumeId,
        [FromBody] SetDaytimeVoteDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _voteService.SetDaytimeVoteAsync(perfumeId, userId ?? 0, dto.Daytime);
        return NoContent();
    }
}
