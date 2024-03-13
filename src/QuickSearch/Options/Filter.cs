namespace QuickSearch.Options;

public class Filter
{
    public FilterType FilterType { get; internal set; }
    public object? Value { get; internal set; }

    public Filter(FilterType filterType, object? value)
    {
        FilterType = filterType;
        Value = value;
    }
}
