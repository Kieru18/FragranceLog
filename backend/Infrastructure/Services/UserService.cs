using Core.DTOs;
using Core.Exceptions;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly FragranceLogContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private const string DeletedEmailDomain = "fragrance.log";

    public UserService(
        FragranceLogContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserProfileDto> GetMeAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => new UserProfileDto
            {
                Id = u.UserId,
                DisplayName = u.Username,
                Email = u.Email
            })
            .SingleOrDefaultAsync();

        if (user == null)
            throw new NotFoundException("User not found.");

        return user;
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            throw new NotFoundException("User not found.");

        if (user.Email != dto.Email)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.UserId != userId);

            if (emailExists)
                throw new ConflictException("Email already in use.");
        }

        if (user.Username != dto.DisplayName)
        {
            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == dto.DisplayName && u.UserId != userId);

            if (usernameExists)
                throw new ConflictException("Username already in use.");
        }

        user.Username = dto.DisplayName;
        user.Email = dto.Email;

        await _context.SaveChangesAsync();

        return new UserProfileDto
        {
            Id = user.UserId,
            DisplayName = user.Username,
            Email = user.Email
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            throw new NotFoundException("User not found.");

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.Password))
            throw new ValidationException("Incorrect password.");

        user.Password = _passwordHasher.Hash(dto.NewPassword);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int userId)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return;

        await using var tx = await _context.Database.BeginTransactionAsync();

        var lists = await _context.PerfumeLists.Where(l => l.UserId == userId).ToListAsync();
        if (lists.Count > 0)
        {
            _context.PerfumeLists.RemoveRange(lists);
            await _context.SaveChangesAsync();
        }

        user.Username = $"deleted_user_{user.UserId}";
        user.Email = $"deleted+{user.UserId}@{DeletedEmailDomain}";
        user.Password = GenerateUnusablePassword(64);

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private static string GenerateUnusablePassword(int length)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);

        var s = Convert.ToBase64String(bytes);

        if (s.Length >= length)
            return s.Substring(0, length);

        var sb = new StringBuilder(s, length);
        while (sb.Length < length)
        {
            var extra = new byte[16];
            RandomNumberGenerator.Fill(extra);
            sb.Append(Convert.ToBase64String(extra));
        }

        return sb.ToString(0, length);
    }
}
