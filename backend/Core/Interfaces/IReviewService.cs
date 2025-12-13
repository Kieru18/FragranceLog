using Core.DTOs;

namespace Core.Interfaces
{
    public interface IReviewService
    {
        Task CreateOrUpdateAsync(int userId, CreateReviewDto dto);
    }
}
