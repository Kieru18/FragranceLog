using Core.DTOs;

namespace Core.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetMeAsync(int userId);
        Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task DeleteAccountAsync(int userId);
    }
}
