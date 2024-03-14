using QuickSearch.Extensions;
using QuickSearch.Mapping;
using QuickSearch.Pagination;
using System.Linq.Expressions;
using System.Net;
using System.Text;

namespace QuickSearch.Sort;

public class SortOptions<TEntity>
    where TEntity : class
{
    internal readonly Dictionary<PropertyKey, Sorter> Sorters = new();
    internal readonly ParameterExpression Parameter = Expression.Parameter(typeof(TEntity), "item");

    public bool TryAddSort(string propertyPath, SortDirection sortDirection)
    {
        if (!Parameter.TryExtractMemberExpression(propertyPath, out var expression))
            return false;

        var sorterKey = new PropertyKey(propertyPath, expression);
        var sorterValue = new Sorter(sortDirection);

        AddSort(sorterKey, sorterValue);

        return true;
    }

    public SortOptions<TEntity> AddSort<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, SortDirection sortDirection)
    {
        var expression = Parameter.ExtractMemberExpression(propertyExpression);
        var sorterKey = new PropertyKey(expression.ExtractPropertyPath(), expression);
        var sorterValue = new Sorter(sortDirection);

        AddSort(sorterKey, sorterValue);

        return this;
    }


    internal SortOptions<TEntity> AddSort(PropertyKey sorterKey, Sorter sorterValue)
    {
        if (Sorters.ContainsKey(sorterKey))
            throw new InvalidOperationException("Cannot multiple sort directions for one path");

        Sorters.Add(sorterKey, sorterValue);

        return this;
    }
    internal StringBuilder ToQueryStringBuilder(string prefix)
        => ToQueryStringBuilder(new StringBuilder(), prefix);

    internal StringBuilder ToQueryStringBuilder(StringBuilder builder, string prefix)
    {
        var taken = 0;
        var maxElements = Sorters.Keys.Count;
        foreach (var sorter in Sorters)
        {
            var queryValue = WebUtility.UrlEncode(sorter.Value.Direction.ToString());
            builder.Append(prefix).Append('.').Append(sorter.Key.Path).Append('=').Append(queryValue);

            if (++taken < maxElements)
                builder.Append('&');
        }

        return builder;
    }

    public string ToQueryString(string prefix)
        => ToQueryStringBuilder(prefix).ToString();

    public bool Any()
        => Sorters.Any();

    public SortOptions<TResult> MapTo<TResult>()
        where TResult : class
        => QuickSearchMapper.MapOptions<TResult, TEntity>(this);
}
