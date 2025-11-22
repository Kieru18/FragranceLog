using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly FragranceLogContext _context;
        private readonly PasswordHasher _hasher;
        private readonly JwtService _jwt;

        public AuthService(
            FragranceLogContext context,
            PasswordHasher hasher,
            JwtService jwt)
        {
            _context = context;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var exists = await _context.Users.AnyAsync(u =>
                u.Email == dto.Email || u.Username == dto.Username);

            if (exists)
                throw new InvalidOperationException("User already exists.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = _hasher.Hash(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var access = _jwt.GenerateAccessToken(user);
            var refresh = _jwt.GenerateRefreshToken(user.UserId);

            return new AuthResponseDto
            {
                AccessToken = access,
                RefreshToken = refresh
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == dto.UsernameOrEmail ||
                    u.Email == dto.UsernameOrEmail) ?? throw new UnauthorizedAccessException();

            if (!_hasher.Verify(dto.Password, user.Password))
                throw new UnauthorizedAccessException();

            var access = _jwt.GenerateAccessToken(user);
            var refresh = _jwt.GenerateRefreshToken(user.UserId);

            return new AuthResponseDto
            {
                AccessToken = access,
                RefreshToken = refresh
            };
        }

        public async Task<AuthResponseDto> RefreshAsync(string refreshToken)
        {
            var principal = _jwt.ValidateRefreshToken(refreshToken) ?? throw new UnauthorizedAccessException();
            var claim = principal.Claims.FirstOrDefault(c => c.Type == "sub") ?? throw new UnauthorizedAccessException();

            if (!int.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException();

            var user = await _context.Users.FindAsync(userId) ?? throw new UnauthorizedAccessException();
            var access = _jwt.GenerateAccessToken(user);
            var newRefresh = _jwt.GenerateRefreshToken(user.UserId);

            return new AuthResponseDto
            {
                AccessToken = access,
                RefreshToken = newRefresh
            };
        }
    }
}
