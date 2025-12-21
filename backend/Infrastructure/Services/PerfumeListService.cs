using Core.Dtos;
using Core.DTOs;
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

    public async Task<IReadOnlyList<PerfumeListDto>> GetUserListsAsync(int userId)
    {
        return await _context.PerfumeLists
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.IsSystem)
            .ThenBy(l => l.Name)
            .Select(l => new PerfumeListDto
            {
                PerfumeListId = l.PerfumeListId,
                Name = l.Name,
                IsSystem = l.IsSystem,
                CreationDate = l.CreationDate
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PerfumeListOverviewDto>> GetListsOverviewAsync(int userId)
    {
        var result = await _context.PerfumeLists
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .Select(l => new PerfumeListOverviewDto
            {
                PerfumeListId = l.PerfumeListId,
                Name = l.Name,
                IsSystem = l.IsSystem,

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
                    .Take(4)
                    .ToList()
            })
            .OrderBy(x => x.IsSystem)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return result;
    }

    public async Task<PerfumeListDto> CreateListAsync(int userId, string name)
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

        return new PerfumeListDto
        {
            PerfumeListId = list.PerfumeListId,
            Name = list.Name,
            IsSystem = list.IsSystem,
            CreationDate = list.CreationDate
        };
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

    public async Task<IReadOnlyList<PerfumeListItemDto>> GetListPerfumesAsync(int userId, int perfumeListId)
    {
        await EnsureListOwnershipAsync(userId, perfumeListId);

        var query =
            from li in _context.PerfumeListItems.AsNoTracking()
            where li.PerfumeListId == perfumeListId
            join p in _context.Perfumes.AsNoTracking() on li.PerfumeId equals p.PerfumeId
            join r in _context.Reviews on p.PerfumeId equals r.PerfumeId into ratings
            join photo in _context.PerfumePhotos on p.PerfumeId equals photo.PerfumeId into photos
            from photo in photos.DefaultIfEmpty()
            select new
            {
                Perfume = p,
                PhotoPath = photo.Path,
                AvgRating = ratings.Any() ? ratings.Average(x => x.Rating) : 0,
                RatingCount = ratings.Count(),
                MyRating = ratings
                    .Where(x => x.UserId == userId)
                    .Select(x => (int?)x.Rating)
                    .FirstOrDefault()
            };

        return await query
            .Select(x => new PerfumeListItemDto
            {
                PerfumeId = x.Perfume.PerfumeId,
                Name = x.Perfume.Name,
                Brand = x.Perfume.Brand.Name,
                ImageUrl = x.PhotoPath,
                AvgRating = Math.Round(x.AvgRating, 2),
                RatingCount = x.RatingCount,
                MyRating = x.MyRating
            })
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

    public async Task<IReadOnlyList<PerfumeListMembershipDto>> GetListsForPerfumeAsync(int userId, int perfumeId)
    {
        var query =
            from l in _context.PerfumeLists.AsNoTracking()
            where l.UserId == userId
            select new PerfumeListMembershipDto
            {
                PerfumeListId = l.PerfumeListId,
                Name = l.Name,
                IsSystem = l.IsSystem,
                ContainsPerfume = _context.PerfumeListItems
                    .Any(li =>
                        li.PerfumeListId == l.PerfumeListId &&
                        li.PerfumeId == perfumeId)
            };

        return await query
            .OrderBy(x => x.IsSystem)
            .ThenBy(x => x.Name)
            .ToListAsync();
    }
}
