using System.Diagnostics.CodeAnalysis;

namespace QuickSearch.Extensions;

internal static class TypeExtensions
{

    [return: NotNullIfNotNull("value")]
    internal static object? ConvertObject(this Type type, object? value)
    {
        if (value == null)
            return null;

        var targetType = type;
        if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))       
            targetType = Nullable.GetUnderlyingType(type)!;

        return Convert.ChangeType(value, targetType);
    }
}
