using Core.DTOs;

namespace Core.Interfaces
{
    public interface IReviewService
    {
        Task CreateOrUpdateAsync(int userId, SaveReviewDto dto);
    }
}
