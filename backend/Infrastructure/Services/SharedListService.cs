using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class SharedListService : ISharedListService
{
    private readonly FragranceLogContext _context;
    private readonly IPerfumeListService _perfumeListService;

    public SharedListService(
        FragranceLogContext context,
        IPerfumeListService perfumeListService)
    {
        _context = context;
        _perfumeListService = perfumeListService;
    }

    public async Task<SharedListDto> ShareAsync(int ownerUserId, int perfumeListId)
    {
        var ownsList = await _context.PerfumeLists
            .AnyAsync(x => x.PerfumeListId == perfumeListId && x.UserId == ownerUserId);

        if (!ownsList)
            throw new UnauthorizedAccessException("List not found or not owned by user.");

        var existing = await _context.SharedLists
            .FirstOrDefaultAsync(x =>
                x.PerfumeListId == perfumeListId &&
                x.ExpirationDate == null);

        if (existing != null)
        {
            return MapToDto(existing);
        }

        var entity = new SharedList
        {
            PerfumeListId = perfumeListId,
            OwnerUserId = ownerUserId,
            ShareToken = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            ExpirationDate = null
        };

        _context.SharedLists.Add(entity);
        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }


    public async Task<SharedListPreviewDto> GetPreviewAsync(Guid shareToken)
    {
        var shared = await _context.SharedLists
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ShareToken == shareToken &&
                x.ExpirationDate == null);

        if (shared == null)
            throw new KeyNotFoundException("Shared list not found or expired.");

        var list = await _context.PerfumeLists
            .AsNoTracking()
            .Where(l => l.PerfumeListId == shared.PerfumeListId)
            .Select(l => new
            {
                l.Name,
                OwnerName = l.User.Username,

                PerfumeCount = _context.PerfumeListItems
                    .Count(li => li.PerfumeListId == l.PerfumeListId),

                PreviewImages = _context.PerfumePhotos
                    .Where(pp =>
                        _context.PerfumeListItems
                            .Where(li => li.PerfumeListId == l.PerfumeListId)
                            .Select(li => li.PerfumeId)
                            .Contains(pp.PerfumeId))
                    .OrderBy(pp => pp.PerfumeId)
                    .Select(pp => pp.Path)
                    .Take(6)
                    .ToList()
            })
            .SingleAsync();

        return new SharedListPreviewDto
        {
            ShareToken = shared.ShareToken,
            ListName = list.Name,
            OwnerName = list.OwnerName,
            PerfumeCount = list.PerfumeCount,
            PreviewImages = list.PreviewImages
        };
    }


    public async Task<int> ImportAsync(int targetUserId, Guid shareToken)
    {
        var shared = await _context.SharedLists
            .Include(x => x.PerfumeList)
            .ThenInclude(x => x.PerfumeListItems)
            .FirstOrDefaultAsync(x =>
                x.ShareToken == shareToken &&
                x.ExpirationDate == null);

        if (shared == null)
            throw new KeyNotFoundException("Shared list not found or expired.");

        var baseName = shared.PerfumeList.Name;
        var finalName = await ResolveListNameAsync(targetUserId, baseName);

        var created = await _perfumeListService.CreateListAsync(
            targetUserId,
            finalName
        );

        foreach (var item in shared.PerfumeList.PerfumeListItems)
        {
            await _perfumeListService.AddPerfumeToListAsync(
                targetUserId,
                created.PerfumeListId,
                item.PerfumeId
            );
        }

        return created.PerfumeListId;
    }


    private static SharedListDto MapToDto(SharedList entity)
        => new()
        {
            ShareToken = entity.ShareToken,
            CreatedAt = entity.CreatedAt,
            ExpirationDate = entity.ExpirationDate
        };

    private async Task<string> ResolveListNameAsync(int userId, string baseName)
    {
        var existingNames = await _context.PerfumeLists
            .Where(x => x.UserId == userId)
            .Select(x => x.Name)
            .ToListAsync();

        if (!existingNames.Contains(baseName))
            return baseName;

        var i = 1;
        while (true)
        {
            var candidate = $"{baseName} ({i})";
            if (!existingNames.Contains(candidate))
                return candidate;
            i++;
        }
    }
}
