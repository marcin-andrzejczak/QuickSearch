using QuickSearch.Options;
using System.Linq.Expressions;

namespace QuickSearch.Extensions;

public static class QueryableSortingExtensions
{
    public static IOrderedQueryable<TEntity> Sorted<TEntity>(
        this IQueryable<TEntity> query,
        SortOptions<TEntity>? sort
    ) where TEntity : class
    {
        if (sort is null)
            return (IOrderedQueryable<TEntity>)query;

        var keys = sort.Sorters.Keys.ToList();

        for (var i = 0; i < sort.Sorters.Keys.Count; i++)
        {
            var direction = sort.Sorters[keys[i]].Direction;
            var orderMethod = GetOrderMethod(i, direction);
            if (string.IsNullOrEmpty(orderMethod))
                continue;

            query = query.OrderByColumnUsing(sort.Parameter, keys[i].Expression, orderMethod);
        }

        return (IOrderedQueryable<TEntity>) query;
    }

    private static IOrderedQueryable<TEntity> OrderByColumnUsing<TEntity>(
        this IQueryable<TEntity> source,
        ParameterExpression parameter,
        Expression expression,
        string method
    )
    {
        var keySelector = Expression.Lambda(expression, parameter);
        var methodCall = Expression.Call(
            typeof(Queryable),
            method,
            new[] { parameter.Type, expression.Type },
            source.Expression,
            Expression.Quote(keySelector)
        );

        return (IOrderedQueryable<TEntity>) source.Provider.CreateQuery(methodCall);
    }

    private static string GetOrderMethod(int expressionIndex, SortDirection direction)
        => (expressionIndex, direction) switch
        {
            (0, SortDirection.Asc) => nameof(Queryable.OrderBy),
            (0, SortDirection.Desc) => nameof(Queryable.OrderByDescending),
            ( > 0, SortDirection.Asc) => nameof(Queryable.ThenBy),
            ( > 0, SortDirection.Desc) => nameof(Queryable.ThenByDescending),
            (_, _) => string.Empty
        };
}

