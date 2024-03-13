namespace QuickSearch.Options;

public class Sorter
{
    public SortDirection Direction { get; internal set; }

    public Sorter(SortDirection direction)
    {
        Direction = direction;
    }
}
