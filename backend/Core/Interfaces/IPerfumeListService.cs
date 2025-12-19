using Core.Entities;

namespace Core.Interfaces;

public interface IPerfumeListService
{
    Task<IReadOnlyList<PerfumeList>> GetUserListsAsync(int userId);
    Task<PerfumeList> CreateListAsync(int userId, string name);
    Task RenameListAsync(int userId, int perfumeListId, string newName);
    Task DeleteListAsync(int userId, int perfumeListId);
    Task<IReadOnlyList<Perfume>> GetListPerfumesAsync(int userId, int perfumeListId);
    Task AddPerfumeToListAsync(int userId, int perfumeListId, int perfumeId);
    Task RemovePerfumeFromListAsync(int userId, int perfumeListId, int perfumeId);
}
