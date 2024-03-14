using System.Linq.Expressions;

namespace QuickSearch.Pagination;

public class PropertyKey : IEquatable<PropertyKey>
{
    public string Path { get; internal set; }

    internal MemberExpression Expression { get; set; }

    internal PropertyKey(string path, MemberExpression expression)
    {
        Path = path;
        Expression = expression;
    }

    public bool Equals(PropertyKey? other)
        => Path == other?.Path;

    public override int GetHashCode()
        => Path.GetHashCode();

    public override string ToString()
        => Path;
}
