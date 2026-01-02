using Core.DTOs;
using Core.Exceptions;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly FragranceLogContext _context;
    private readonly PasswordHasher _passwordHasher;

    public UserService(
        FragranceLogContext context,
        PasswordHasher passwordHasher)
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
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
