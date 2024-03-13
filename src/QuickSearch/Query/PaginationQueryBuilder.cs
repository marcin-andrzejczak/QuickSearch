using QuickSearch.Options;
using System.Text;

namespace QuickSearch.Query;

public class PaginationQueryBuilder<TEntity>
    where TEntity : class
{
    public string? PagePrefix { get; private set; }
    public PageOptions? PageOptions { get; private set; }

    public string? FilterPrefix { get; private set; }
    public FilterOptions<TEntity>? FilterOptions { get; private set; }

    public string? SortPrefix { get; private set; }
    public SortOptions<TEntity>? SortOptions { get; private set; }

    public PaginationQueryBuilder<TEntity> Page(string prefix, PageOptions page)
    {
        PagePrefix = prefix ?? throw new ArgumentNullException(nameof(prefix), "Prefix is required");
        PageOptions = page ?? throw new ArgumentNullException(nameof(page), "Options are required");

        return this;
    }

    public PaginationQueryBuilder<TEntity> Filter(string prefix, FilterOptions<TEntity> filter)
    {
        FilterPrefix = prefix ?? throw new ArgumentNullException(nameof(prefix), "Prefix is required");
        FilterOptions = filter ?? throw new ArgumentNullException(nameof(filter), "Options are required");

        return this;
    }

    public PaginationQueryBuilder<TEntity> Sort(string prefix,  SortOptions<TEntity> sort)
    {
        SortPrefix = prefix ?? throw new ArgumentNullException(nameof(prefix), "Prefix is required");
        SortOptions = sort ?? throw new ArgumentNullException(nameof(sort), "Options are required");
        
        return this;
    }

    public string ToQueryString()
    {
        var builder = new StringBuilder();

        if (PageOptions is not null)
            builder.Append(PageOptions.ToQueryStringBuilder(PagePrefix!));

        if (FilterOptions is not null)
        {
            if (builder.Length > 0)
                builder.Append('&');

            builder.Append(FilterOptions.ToQueryStringBuilder(FilterPrefix!));
        }

        if (SortOptions is not null)
        {
            if (builder.Length > 0)
                builder.Append('&');

            builder.Append(SortOptions.ToQueryStringBuilder(SortPrefix!));
        }

        return builder.ToString();
    }
}
