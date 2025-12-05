using Core.DTOs;
using Core.Entities;
using Core.Exceptions;
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
                JwtService jwt
            )
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
                throw new ConflictException("User already exists.");

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
                    u.Email == dto.UsernameOrEmail) ?? throw new UnauthorizedException("User does not exist.");

            if (!_hasher.Verify(dto.Password, user.Password))
                throw new UnauthorizedException("Invalid credentials.");

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
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ValidationException("Refresh token missing.");

            var principal = _jwt.ValidateRefreshToken(refreshToken) ?? throw new UnauthorizedException("Invalid refresh token.");

            var claim = principal.Claims.FirstOrDefault(c => c.Type == "sub") ?? throw new UnauthorizedException("Invalid refresh token.");

            if (!int.TryParse(claim.Value, out var userId))
                throw new UnauthorizedException("Invalid refresh token.");

            var user = await _context.Users.FindAsync(userId) ?? throw new UnauthorizedException("User not found.");
            var newAccess = _jwt.GenerateAccessToken(user);
            var newRefresh = _jwt.GenerateRefreshToken(user.UserId);


            return new AuthResponseDto
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh
            };
        }
    }
}
