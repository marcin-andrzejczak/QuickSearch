namespace QuickSearch.Models;

public class Page<TItem> where TItem : class
{
    public IEnumerable<TItem> Items { get; internal set; } = Enumerable.Empty<TItem>();
    public int CurrentPage { get; internal set; }
    public int PageSize { get; internal set; }
    public int TotalItems { get; internal set; }
    public int TotalPages => (int) Math.Ceiling((double)TotalItems / PageSize);
    
    public Page(IEnumerable<TItem> items, int currentPage, int pageSize, int totalItems)
    {
        Items = items;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalItems = totalItems;
    }

    public Page<TResult> MapTo<TResult>(Func<TItem, TResult> mapper)
        where TResult : class
    {
        var mappedItems = Items.Select(mapper);
        return new Page<TResult>(mappedItems, CurrentPage, PageSize, TotalItems);
    }

}

