using Microsoft.EntityFrameworkCore;

namespace Application.Common.Pagination.Models;

public class PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public Pagination Pagination { get; }

    public PaginatedList(IReadOnlyCollection<T> items, Pagination pagination)
    {
        Items = items;
        Pagination = pagination;
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        var totalPages = (int)Math.Ceiling(count / (double)pageSize);

        var pagination = new Pagination
        {
            PageNumber = pageNumber,
            TotalPages = totalPages,
            TotalCount = count
        };

        return new PaginatedList<T>(items, pagination);
    }
}
public class Pagination
{
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
