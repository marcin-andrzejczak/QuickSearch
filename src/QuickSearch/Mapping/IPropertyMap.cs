using QuickSearch.Options;
using System.Linq.Expressions;

namespace QuickSearch.Mapping;

public interface IPropertyMap
{
    public PropertyKey ApplyMap(PropertyKey srcPropertyKey, ParameterExpression parameter);
}
