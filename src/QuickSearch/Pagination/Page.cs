namespace QuickSearch.Pagination;

public class Page<TItem> where TItem : class
{
    public IEnumerable<TItem> Items { get; private set; }
    public int CurrentPage { get; private set; }
    public int PageSize { get; private set; }
    public int TotalItems { get; private set; }
    public int TotalPages { get; private set; }

    public Page(IEnumerable<TItem> items, int currentPage, int pageSize, int totalItems)
    {
        Items = items ?? Enumerable.Empty<TItem>();
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public Page<TResult> MapTo<TResult>(Func<TItem, TResult> mapper)
        where TResult : class
    {
        var mappedItems = Items.Select(mapper);
        return new Page<TResult>(mappedItems, CurrentPage, PageSize, TotalItems);
    }

}

