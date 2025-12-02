//using Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        private SymmetricSecurityKey GetKey()
        {
            var key = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key not present in configuration.");

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }

        public ClaimsPrincipal? ValidateAccessToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = GetKey();

                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                return handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        //public string GenerateAccessToken(User user)
        //{
        //    var key = GetKey();
        //    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var claims = new List<Claim>
        //    {
        //        new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        //        new(JwtRegisteredClaimNames.UniqueName, user.Username),
        //        new(JwtRegisteredClaimNames.Email, user.Email)
        //    };

        //    var token = new JwtSecurityToken(
        //        issuer: _config["Jwt:Issuer"],
        //        audience: _config["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddMinutes(30),
        //        signingCredentials: credentials
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        public ClaimsPrincipal? ValidateRefreshToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = GetKey();

                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                return handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        public string GenerateRefreshToken(int userId)
        {
            var key = GetKey();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
