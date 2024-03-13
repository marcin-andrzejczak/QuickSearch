using QuickSearch.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QuickSearch.Binding;

public class FilterOptionsModelBinder<TEntity> : IModelBinder
    where TEntity : class
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var queryItems = bindingContext.HttpContext
            .Request.Query
            .Where(queryItem => queryItem.Key.StartsWith(bindingContext.ModelName, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        var filterOptions = new FilterOptions<TEntity>();
        var entityType = bindingContext.ModelType.GenericTypeArguments.First();

        var isValid = true;
        foreach (var (queryKey, queryValues) in queryItems)
        {
            var firstDelimiterIndex = queryKey.IndexOf('.');
            var lastDelimiterIndex = queryKey.LastIndexOf('.');
            if (firstDelimiterIndex == -1 || firstDelimiterIndex == lastDelimiterIndex)
            {
                isValid = false;
                var message = "Invalid filter data, missing property or filter";
                AddModelError(bindingContext.ModelState, FilterModelBindingErrors.MissingPropertyOrFilter, entityType, queryKey, message);
                continue;
            }

            var propertyPath = queryKey.Substring(firstDelimiterIndex + 1, lastDelimiterIndex - firstDelimiterIndex - 1);
            var filterValue = queryKey.Substring(lastDelimiterIndex + 1);

            if (!Enum.TryParse<FilterType>(filterValue, true, out var filterType))
            {
                isValid = false;
                var message = "Invalid filter type";
                AddModelError(bindingContext.ModelState, FilterModelBindingErrors.UnrecognizedFilter, entityType, queryKey, message);
            }

            var filters = queryValues
                .Select(v => v == "null" ? null : v)
                .ToList();

            if (!filterOptions.TryAddFilters(propertyPath, filterType, filters))
            {
                isValid = false;
                var message = $"Property does not exist on entity '{entityType.Name}'";
                AddModelError(bindingContext.ModelState, FilterModelBindingErrors.UnrecognizedProperty, entityType, queryKey, message);
                continue;
            }
        }

        if (isValid && filterOptions.Any())
            bindingContext.Result = ModelBindingResult.Success(filterOptions);
        else
            bindingContext.Result = ModelBindingResult.Failed();

        return Task.CompletedTask;
    }

    public virtual void AddModelError(
        ModelStateDictionary modelStateDictionary,
        FilterModelBindingErrors errorType,
        Type entityType,
        string key,
        string message)
        => modelStateDictionary.AddModelError(key, message);
}

public enum FilterModelBindingErrors
{
    MissingPropertyOrFilter,
    UnrecognizedProperty,
    UnrecognizedFilter
}
