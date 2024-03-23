using Microsoft.EntityFrameworkCore;

namespace QuickSearch.Pagination;

public static class QueryablePagingExtensions
{
    public static Page<TEntity> ToPage<TEntity>(
        this IQueryable<TEntity> query,
        PageOptions? page
    ) where TEntity : class
    {
        page ??= new PageOptions();

        var skipEntities = (page.Number - 1) * page.Size;
        var entities = query
            .Skip(skipEntities)
            .Take(page.Size)
            .ToList();

        var totalItems = query.Count();
        return new Page<TEntity>(
            entities,
            page.Number,
            page.Size,
            totalItems
        );
    }

    public static async Task<Page<TEntity>> ToPageAsync<TEntity>(
        this IQueryable<TEntity> query,
        PageOptions? page,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        page ??= new PageOptions();

        var skipEntities = (page.Number - 1) * page.Size;
        var entities = await query
            .Skip(skipEntities)
            .Take(page.Size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalItems = await query
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new Page<TEntity>(
            entities,
            page.Number,
            page.Size,
            totalItems
        );
    }

}
