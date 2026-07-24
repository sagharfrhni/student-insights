using Microsoft.EntityFrameworkCore;

namespace StudentInsights.Application.Common.Models;

/// <summary>
/// Generic paged result reused across all list queries. Provides a shared
/// response shape and helper methods for efficient server-side pagination
/// with Entity Framework Core.
/// </summary>
public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages =>
        PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedResult(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static async Task<PaginatedResult<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    public PaginatedResult<TResult> Map<TResult>(
        Func<T, TResult> selector)
    {
        return new PaginatedResult<TResult>(
            Items.Select(selector).ToList(),
            PageNumber,
            PageSize,
            TotalCount);
    }
}