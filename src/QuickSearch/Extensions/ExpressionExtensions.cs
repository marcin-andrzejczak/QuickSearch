using System.Linq.Expressions;

namespace QuickSearch.Extensions;

internal static class ExpressionExtensions
{
    internal static string ExtractPropertyPath<TOut, TIn>(this Expression<Func<TOut, TIn>> expression)
    {
        if (expression.Body is not MemberExpression member)
            throw new InvalidOperationException("Expression must be of type MemberExpression");

        return member.ExtractPropertyPath();
    }

    internal static string ExtractPropertyPath(this MemberExpression expression)
    {
        var memberExpression = expression;
        var propertyPath = string.Empty;
        while (memberExpression is not null)
        {
            propertyPath = $".{memberExpression.Member.Name}{propertyPath}";
            memberExpression = memberExpression.Expression as MemberExpression;
        }

        return propertyPath.TrimStart('.');
    }

    internal static MemberExpression ExtractMemberExpression<TOut, TIn>(
        this ParameterExpression parameter,
        Expression<Func<TOut, TIn>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
            throw new InvalidOperationException("Expression must be of type MemberExpression");

        var members = new Stack<string>();

        members.Push(memberExpression.Member.Name);

        while (memberExpression.Expression is MemberExpression exp)
        {
            memberExpression = exp;
            members.Push(memberExpression.Member.Name);
        }

        return (MemberExpression)members
            .Aggregate((Expression)parameter, Expression.PropertyOrField);
    }

    internal static bool TryExtractMemberExpression(
        this ParameterExpression parameter,
        string propertyPath,
        out MemberExpression expression)
    {
        expression = default!;

        try
        {
            var exp = propertyPath
                .Split('.')
                .Aggregate((Expression)parameter, Expression.PropertyOrField);

            if (exp is MemberExpression member)
                expression = member;

            return expression is not null;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
