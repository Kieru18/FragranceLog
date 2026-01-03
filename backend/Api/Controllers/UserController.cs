using Core.DTOs;
using Core.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/user")]
public sealed class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Me()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        return Ok(await _service.GetMeAsync(userId.Value));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateProfileDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        return Ok(await _service.UpdateProfileAsync(userId.Value, dto));
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _service.ChangePasswordAsync(userId.Value, dto);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _service.DeleteAccountAsync(userId.Value);
        return NoContent();
    }
}
