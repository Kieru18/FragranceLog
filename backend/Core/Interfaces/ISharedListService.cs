using Core.DTOs;

namespace Core.Interfaces;

public interface ISharedListService
{
    /// <summary>
    /// Creates or reuses an active share for a list owned by the user.
    /// Ensures at most one active share per list.
    /// </summary>
    Task<SharedListDto> ShareAsync(int ownerUserId, int perfumeListId);

    /// <summary>
    /// Returns a preview of a shared list by token.
    /// Throws if token is invalid or expired.
    /// </summary>
    Task<SharedListPreviewDto> GetPreviewAsync(Guid shareToken);

    /// <summary>
    /// Imports a shared list into the requesting user's lists.
    /// Creates a new list with name collision handling.
    /// </summary>
    Task<int> ImportAsync(int targetUserId, Guid shareToken);
}
