using QuickSearch.Options;
using System.Linq.Expressions;

namespace QuickSearch.Extensions;

public static class QueryableFilteringExtensions
{
    private static readonly Dictionary<FilterType, Func<Expression, Expression, Expression>> FilterMap = new()
    {
        { FilterType.Lt, Expression.LessThan },
        { FilterType.Lte, Expression.LessThanOrEqual },
        { FilterType.Gt, Expression.GreaterThan },
        { FilterType.Gte, Expression.GreaterThanOrEqual },
        { FilterType.Eq, Expression.Equal },
        { FilterType.Neq, Expression.NotEqual },
        { FilterType.Like, ExpressionContains },
        { FilterType.Nlike, ExpressionNotContains }
    };

    public static IQueryable<TEntity> Filtered<TEntity>(
        this IQueryable<TEntity> query,
        FilterOptions<TEntity>? filter
    ) where TEntity : class
    {
        if (filter is null)
            return query;

        Expression? queryExpression = null;
        
        foreach (var (key, filters) in filter.Filters)
        {
            var filterGroups = filters.GroupBy(f => f.FilterType);
            foreach (var filterGroup in filterGroups)
            {
                var filterExpression = FilterMap.GetValueOrDefault(filterGroup.Key);
                if (filterExpression is null)
                    continue;

                var filterValues = filterGroup.Select(f => f.Value);
                var filterBody  = CreateOr(key.Expression, filterValues, filterExpression);

                queryExpression = queryExpression is null
                    ? filterBody
                    : Expression.And(queryExpression, filterBody);   
            }
        }

        if (queryExpression is null)
            return query;

        var lambda = Expression.Lambda<Func<TEntity, bool>>(queryExpression, filter.Parameter);

        return query.Where(lambda);
    }

    private static Expression CreateOr<TValue>(
        Expression member,
        IEnumerable<TValue> items,
        Func<Expression, Expression, Expression> comparison
    )
    {
        var constants = items
            .Select(i => Expression.Convert(Expression.Constant(i), member.Type))
            .Cast<Expression>()
            .ToList();

        var expression = comparison(member, constants.First());
        for (var i = 1; i < constants.Count; i++)
        {
            var nextExpression = constants[i];

            expression = Expression.Or(
                expression,
                comparison(member, nextExpression)
            );
        }

        return expression;
    }

    static Expression ExpressionContains(Expression left, Expression right)
    {
        var method = typeof(string)
            .GetMethod(nameof(string.Contains), new[] { typeof(string) });

        return Expression.Call(left, method!, right);
    }

    static Expression ExpressionNotContains(Expression left, Expression right)
        => Expression.Not(ExpressionContains(left, right));

}
