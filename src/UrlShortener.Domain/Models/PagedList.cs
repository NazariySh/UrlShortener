using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Domain.Models;

public class PagedList<T>
{
    public List<T> Items { get; set; } = [];
    public ushort PageNumber { get; set; }
    public ushort PageSize { get; set; }
    public ushort TotalPages { get; set; }
    public ushort TotalCount { get; set; }

    public PagedList()
    {
    }

    public PagedList(List<T> items, ushort count, ushort pageNumber, ushort pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (ushort)Math.Ceiling(count / (double)pageSize);
    }

    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source,
        ushort pageNumber,
        ushort pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, (ushort)1);
        pageSize = Math.Max(pageSize, (ushort)1);

        var count = (ushort)await source.CountAsync(cancellationToken);
        pageSize = Math.Min(pageSize, count);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}