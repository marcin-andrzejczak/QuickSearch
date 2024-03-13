using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QuickSearch.Options;

namespace QuickSearch.Binding;

public class OptionsBindersProvider : IModelBinderProvider
{
    private readonly Dictionary<Type, Type> _optionTypes = new() {
        { typeof(SortOptions<>), typeof(SortOptionsModelBinder<>)},
        { typeof(FilterOptions<>), typeof(FilterOptionsModelBinder<>) }
    };

    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));

        if (!context.Metadata.ModelType.IsGenericType)
            return null!;

        var modelGenericType = context.Metadata.ModelType.GetGenericTypeDefinition();
        var entityType = context.Metadata.ModelType.GetGenericArguments().First();

        if (!_optionTypes.TryGetValue(modelGenericType, out var binderType))
            return null!;

        var binderGenericType = binderType.MakeGenericType(entityType);
        return new BinderTypeModelBinder(binderGenericType);
    }
}
