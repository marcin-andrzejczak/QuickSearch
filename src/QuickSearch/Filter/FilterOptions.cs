using QuickSearch.Extensions;
using QuickSearch.Mapping;
using QuickSearch.Pagination;
using System.Linq.Expressions;
using System.Net;
using System.Text;

namespace QuickSearch.Filter;

public class FilterOptions<TEntity>
    where TEntity : class
{
    internal readonly Dictionary<PropertyKey, List<Filter>> Filters = new();
    internal readonly ParameterExpression Parameter = Expression.Parameter(typeof(TEntity), "item");

    public bool TryAddFilter<TProperty>(string propertyPath, FilterType filterType, TProperty value)
        where TProperty : IEquatable<TProperty>, IComparable<TProperty>
    {
        if (!Parameter.TryExtractMemberExpression(propertyPath, out var expression))
            return false;

        var filterKey = new PropertyKey(propertyPath, expression);
        var filterValue = new Filter(filterType, expression.Type.ConvertObject(value));

        AddFilter(filterKey, filterValue);

        return true;
    }

    public bool TryAddFilters<TProperty>(string propertyPath, FilterType filterType, List<TProperty?> values)
        where TProperty : IEquatable<TProperty>, IComparable<TProperty>
    {
        if (!Parameter.TryExtractMemberExpression(propertyPath, out var expression))
            return false;

        var filterKey = new PropertyKey(propertyPath, expression);
        var filterValues = values
            .Select(v => new Filter(filterType, expression.Type.ConvertObject(v)))
            .ToList();

        AddFilters(filterKey, filterValues);

        return true;
    }

    public FilterOptions<TEntity> AddFilter<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, FilterType filterType, TProperty value)
    {
        var expression = Parameter.ExtractMemberExpression(propertyExpression);
        var filterKey = new PropertyKey(expression.ExtractPropertyPath(), expression);
        var filterValue = new Filter(filterType, expression.Type.ConvertObject(value));

        AddFilter(filterKey, filterValue);

        return this;
    }

    internal FilterOptions<TEntity> AddFilter(PropertyKey filterKey, Filter filterValue)
    {
        var filters = Filters.TryGetValue(filterKey, out var existingFilters)
            ? existingFilters
            : new List<Filter>();

        if (!Filters.ContainsKey(filterKey))
            Filters.Add(filterKey, filters);

        filters.Add(filterValue);

        return this;
    }

    internal FilterOptions<TEntity> AddFilters(PropertyKey filterKey, List<Filter> filterValue)
    {
        var filters = Filters.TryGetValue(filterKey, out var existingFilters)
            ? existingFilters
            : new List<Filter>();

        if (!Filters.ContainsKey(filterKey))
            Filters.Add(filterKey, filters);

        filters.AddRange(filterValue);

        return this;
    }
    internal StringBuilder ToQueryStringBuilder(string prefix)
        => ToQueryStringBuilder(new StringBuilder(), prefix);

    internal StringBuilder ToQueryStringBuilder(StringBuilder builder, string prefix)
    {
        var taken = 0;
        var maxElements = Filters.Keys.Count;
        foreach (var (filterKey, filters) in Filters)
        {
            maxElements += filters.Count - 1;
            foreach (var filterValue in filters)
            {
                var queryValue = WebUtility.UrlEncode(filterValue.Value?.ToString() ?? "null");
                builder
                    .Append(prefix)
                    .Append('.')
                    .Append(filterKey.Path)
                    .Append('.')
                    .Append(filterValue.FilterType.ToString())
                    .Append('=')
                    .Append(queryValue);

                if (++taken < maxElements)
                    builder.Append('&');
            }
        }

        return builder;
    }

    public string ToQueryString(string prefix)
        => ToQueryStringBuilder(prefix).ToString();

    public bool Any()
        => Filters.Any();

    public FilterOptions<TResult> MapTo<TResult>()
        where TResult : class
        => QuickSearchMapper.Instance.MapOptions<TResult, TEntity>(this);
}
