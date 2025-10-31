using ClipboardPilot.Domain.Entities;
using ClipboardPilot.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace ClipboardPilot.Domain.Interfaces;

public interface IClipboardRepository
{
    Task<ClipboardItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<ClipboardItem>> GetAllAsync();
    Task<IEnumerable<ClipboardItem>> GetRecentAsync(int count);
    Task<IEnumerable<ClipboardItem>> GetPinnedAsync();
    Task<IEnumerable<ClipboardItem>> GetByFavoriteRankAsync(int rank);
    Task<IEnumerable<ClipboardItem>> SearchAsync(string searchText, bool useRegex = false);
    Task<IEnumerable<ClipboardItem>> FilterAsync(
        ClipboardType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? tags = null,
        ColorLabel? label = null,
        bool? pinned = null,
        long? minSize = null,
        long? maxSize = null);
    Task<ClipboardItem?> GetByHashAsync(string hash, DateTime since);
    Task AddAsync(ClipboardItem item);
    Task UpdateAsync(ClipboardItem item);
    Task DeleteAsync(Guid id);
    Task DeleteRangeAsync(IEnumerable<Guid> ids);
    Task DeleteOlderThanAsync(DateTime date);
    Task<int> GetCountAsync();
    Task<bool> ExistsWithFavoriteRankAsync(int rank);
}
