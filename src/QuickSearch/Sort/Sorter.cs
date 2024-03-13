namespace QuickSearch.Sort;

public class Sorter
{
    public SortDirection Direction { get; internal set; }

    public Sorter(SortDirection direction)
    {
        Direction = direction;
    }
}
