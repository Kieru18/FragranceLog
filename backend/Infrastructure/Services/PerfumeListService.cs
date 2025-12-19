using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PerfumeListService : IPerfumeListService
{
    private readonly FragranceLogContext _context;

    public PerfumeListService(FragranceLogContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PerfumeList>> GetUserListsAsync(int userId)
    {
        return await _context.PerfumeLists
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.IsSystem)
            .ThenBy(l => l.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PerfumeList> CreateListAsync(int userId, string name)
    {
        var exists = await _context.PerfumeLists
            .AnyAsync(l => l.UserId == userId && l.Name == name);

        if (exists)
            throw new InvalidOperationException("List with this name already exists.");

        var list = new PerfumeList
        {
            UserId = userId,
            Name = name,
            IsSystem = false,
            CreationDate = DateTime.Now
        };

        _context.PerfumeLists.Add(list);
        await _context.SaveChangesAsync();

        return list;
    }

    public async Task RenameListAsync(int userId, int perfumeListId, string newName)
    {
        var list = await GetOwnedListAsync(userId, perfumeListId);

        if (list.IsSystem)
            throw new InvalidOperationException("System lists cannot be renamed.");

        list.Name = newName;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteListAsync(int userId, int perfumeListId)
    {
        var list = await GetOwnedListAsync(userId, perfumeListId);

        if (list.IsSystem)
            throw new InvalidOperationException("System lists cannot be deleted.");

        _context.PerfumeLists.Remove(list);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Perfume>> GetListPerfumesAsync(int userId, int perfumeListId)
    {
        await EnsureListOwnershipAsync(userId, perfumeListId);

        return await _context.PerfumeListItems
            .Where(i => i.PerfumeListId == perfumeListId)
            .Select(i => i.Perfume)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddPerfumeToListAsync(int userId, int perfumeListId, int perfumeId)
    {
        await EnsureListOwnershipAsync(userId, perfumeListId);

        var exists = await _context.PerfumeListItems
            .AnyAsync(i => i.PerfumeListId == perfumeListId && i.PerfumeId == perfumeId);

        if (exists)
            return;

        _context.PerfumeListItems.Add(new PerfumeListItem
        {
            PerfumeListId = perfumeListId,
            PerfumeId = perfumeId,
            CreationDate = DateTime.Now
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemovePerfumeFromListAsync(int userId, int perfumeListId, int perfumeId)
    {
        await EnsureListOwnershipAsync(userId, perfumeListId);

        var item = await _context.PerfumeListItems
            .FirstOrDefaultAsync(i =>
                i.PerfumeListId == perfumeListId &&
                i.PerfumeId == perfumeId);

        if (item == null)
            return;

        _context.PerfumeListItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureListOwnershipAsync(int userId, int perfumeListId)
    {
        var owns = await _context.PerfumeLists
            .AnyAsync(l => l.PerfumeListId == perfumeListId && l.UserId == userId);

        if (!owns)
            throw new UnauthorizedAccessException();
    }

    private async Task<PerfumeList> GetOwnedListAsync(int userId, int perfumeListId)
    {
        var list = await _context.PerfumeLists
            .FirstOrDefaultAsync(l =>
                l.PerfumeListId == perfumeListId &&
                l.UserId == userId);

        if (list == null)
            throw new UnauthorizedAccessException();

        return list;
    }
}
