using Microsoft.EntityFrameworkCore;
using ClipboardPilot.Domain.Entities;
using ClipboardPilot.Domain.Enums;
using ClipboardPilot.Domain.Interfaces;
using ClipboardPilot.Infrastructure.Data;
using System.Text.RegularExpressions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace ClipboardPilot.Infrastructure.Repositories;

public class ClipboardRepository : IClipboardRepository
{
    private readonly AppDbContext _context;

    public ClipboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ClipboardItem?> GetByIdAsync(Guid id)
    {
        return await _context.ClipboardItems.FindAsync(id);
    }

    public async Task<IEnumerable<ClipboardItem>> GetAllAsync()
    {
        return await _context.ClipboardItems
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClipboardItem>> GetRecentAsync(int count)
    {
        return await _context.ClipboardItems
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClipboardItem>> GetPinnedAsync()
    {
        return await _context.ClipboardItems
            .AsNoTracking()
            .Where(x => x.Pinned)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClipboardItem>> GetByFavoriteRankAsync(int rank)
    {
        return await _context.ClipboardItems
            .AsNoTracking()
            .Where(x => x.FavoriteRank == rank)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClipboardItem>> SearchAsync(string searchText, bool useRegex = false)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return await GetAllAsync();

        var query = _context.ClipboardItems.AsNoTracking();

        if (useRegex)
        {
            var items = await query.ToListAsync();
            try
            {
                var regex = new Regex(searchText, RegexOptions.IgnoreCase);
                return items.Where(x => 
                    (x.Text != null && regex.IsMatch(x.Text)) ||
                    (x.Preview != null && regex.IsMatch(x.Preview)) ||
                    (x.Tags != null && regex.IsMatch(x.Tags))
                ).OrderByDescending(x => x.CreatedAt);
            }
            catch
            {
                return Enumerable.Empty<ClipboardItem>();
            }
        }

        return await query
            .Where(x => 
                (x.Text != null && x.Text.Contains(searchText)) ||
                (x.Preview != null && x.Preview.Contains(searchText)) ||
                (x.Tags != null && x.Tags.Contains(searchText)))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClipboardItem>> FilterAsync(
        ClipboardType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? tags = null,
        ColorLabel? label = null,
        bool? pinned = null,
        long? minSize = null,
        long? maxSize = null)
    {
        var query = _context.ClipboardItems.AsNoTracking();

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt <= toDate.Value);

        if (!string.IsNullOrEmpty(tags))
            query = query.Where(x => x.Tags.Contains(tags));

        if (label.HasValue)
            query = query.Where(x => x.Label == label.Value);

        if (pinned.HasValue)
            query = query.Where(x => x.Pinned == pinned.Value);

        if (minSize.HasValue)
            query = query.Where(x => x.Size >= minSize.Value);

        if (maxSize.HasValue)
            query = query.Where(x => x.Size <= maxSize.Value);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<ClipboardItem?> GetByHashAsync(string hash, DateTime since)
    {
        return await _context.ClipboardItems
            .AsNoTracking()
            .Where(x => x.Hash == hash && x.CreatedAt >= since)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(ClipboardItem item)
    {
        await _context.ClipboardItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClipboardItem item)
    {
        _context.ClipboardItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await _context.ClipboardItems.FindAsync(id);
        if (item != null)
        {
            _context.ClipboardItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<Guid> ids)
    {
        var items = await _context.ClipboardItems
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
        
        _context.ClipboardItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOlderThanAsync(DateTime date)
    {
        var items = await _context.ClipboardItems
            .Where(x => x.CreatedAt < date)
            .ToListAsync();
        
        _context.ClipboardItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.ClipboardItems.CountAsync();
    }

    public async Task<bool> ExistsWithFavoriteRankAsync(int rank)
    {
        return await _context.ClipboardItems.AnyAsync(x => x.FavoriteRank == rank);
    }
}
