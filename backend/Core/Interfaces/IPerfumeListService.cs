using Core.Dtos;
using Core.DTOs;

namespace Core.Interfaces;

public interface IPerfumeListService
{
    Task<IReadOnlyList<PerfumeListDto>> GetUserListsAsync(int userId);
    Task<IReadOnlyList<PerfumeListOverviewDto>> GetListsOverviewAsync(int userId);
    Task<PerfumeListDto> CreateListAsync(int userId, string name);
    Task RenameListAsync(int userId, int perfumeListId, string newName);
    Task DeleteListAsync(int userId, int perfumeListId);
    Task<IReadOnlyList<PerfumeListItemDto>> GetListPerfumesAsync(int userId, int perfumeListId);
    Task AddPerfumeToListAsync(int userId, int perfumeListId, int perfumeId);
    Task RemovePerfumeFromListAsync(int userId, int perfumeListId, int perfumeId);
}
