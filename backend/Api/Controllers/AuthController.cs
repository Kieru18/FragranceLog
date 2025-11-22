using Core.DTOs;
using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly FragranceLogContext _context;
    private readonly PasswordHasher _hasher;
    private readonly JwtService _jwt;

    public AuthController(FragranceLogContext context, PasswordHasher hasher, JwtService jwt)
    {
        _context = context;
        _hasher = hasher;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u =>
                u.Email == dto.Email || u.Username == dto.Username))
            return BadRequest("User already exists.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Password = _hasher.Hash(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Registered");
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == dto.UsernameOrEmail ||
                u.Email == dto.UsernameOrEmail);

        if (user == null) return Unauthorized();
        if (!_hasher.Verify(dto.Password, user.Password)) return Unauthorized();

        var access = _jwt.GenerateAccessToken(user);
        var refresh = _jwt.GenerateRefreshToken(user.UserId);

        return new AuthResponseDto
        {
            AccessToken = access,
            RefreshToken = refresh
        };
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshDto dto)
    {
        var principal = _jwt.ValidateRefreshToken(dto.RefreshToken);
        if (principal == null) return Unauthorized();

        var subClaim = principal.Claims.FirstOrDefault(c => c.Type == "sub");
        if (subClaim == null) return Unauthorized();

        if (!int.TryParse(subClaim.Value, out int userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var newAccess = _jwt.GenerateAccessToken(user);
        var newRefresh = _jwt.GenerateRefreshToken(user.UserId);

        return new AuthResponseDto
        {
            AccessToken = newAccess,
            RefreshToken = newRefresh
        };
    }
}
