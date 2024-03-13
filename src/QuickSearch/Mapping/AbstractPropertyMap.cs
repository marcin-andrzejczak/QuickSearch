using QuickSearch.Extensions;
using QuickSearch.Pagination;
using System.Linq.Expressions;

namespace QuickSearch.Mapping;

public abstract class AbstractPropertyMap<TIn, TOut> : IPropertyMap
    where TIn : class
    where TOut : class
{
    private readonly Dictionary<string, MemberExpression> _propertyMap = new();

    public AbstractPropertyMap<TIn, TOut> Map<TProperty>(Expression<Func<TIn, TProperty>> source, Expression<Func<TOut, TProperty>> target)
    {
        var sourcePath = source.ExtractPropertyPath();

        if (_propertyMap.ContainsKey(sourcePath))
        {
            var existingMap = _propertyMap[sourcePath];
            var targetPath = target.ExtractPropertyPath();
            var error = $"Cannot map property '{sourcePath}' to '{targetPath}', since it is already mapped to {existingMap}";
            throw new InvalidOperationException(error);
        }

        _propertyMap.Add(sourcePath, (MemberExpression) target.Body);

        return this;
    }

    public PropertyKey ApplyMap(PropertyKey sourceKey, ParameterExpression parameter)
    {
        if (_propertyMap.TryGetValue(sourceKey.Path, out var targetExpression))
        {
            var expression = Expression.MakeMemberAccess(parameter, targetExpression!.Member);

            return new PropertyKey(targetExpression.ExtractPropertyPath(), expression);
        }
        else
        {
            var sourcePropertyPath = sourceKey.Expression.ExtractPropertyPath();

            parameter.TryExtractMemberExpression(sourcePropertyPath, out var expression);

            return new PropertyKey(sourcePropertyPath, expression);
        }
    }
}
