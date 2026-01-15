using Core.Entities;
using System.Security.Claims;

namespace Core.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(int userId);
        ClaimsPrincipal? ValidateAccessToken(string token);
        ClaimsPrincipal? ValidateRefreshToken(string token);
    }
}
