using QuickSearch.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.IO;

namespace QuickSearch.Binding;

public class SortOptionsModelBinder<TEntity> : IModelBinder
    where TEntity : class
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var queryItems = bindingContext.HttpContext
            .Request.Query
            .Where(queryItem => queryItem.Key.StartsWith(bindingContext.ModelName, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        var sortOptions = new SortOptions<TEntity>();
        var entityType = bindingContext.ModelType.GenericTypeArguments.First();

        var isValid = true;
        foreach (var (queryKey, queryValues) in queryItems)
        {
            var firstDelimiterIndex = queryKey.IndexOf('.');
            if (firstDelimiterIndex == -1 || firstDelimiterIndex == queryKey.Length - 1)
            {
                isValid = false;
                var message = "Missing entity property";
                AddModelError(bindingContext.ModelState, SortModelBindingErrors.MissingProperty, entityType, queryKey, message);
                continue;
            }

            var propertyPath = queryKey.Substring(firstDelimiterIndex + 1);
            var queryValue = queryValues.ToList();

            if (queryValue.Count > 1 || sortOptions.Sorters.Keys.Any(k => k.Path == propertyPath))
            {
                isValid = false;
                var message = "Property cannot have multiple sorting directions";
                AddModelError(bindingContext.ModelState, SortModelBindingErrors.MultipleSortingDirections, entityType, queryKey, message);
                continue;
            }

            if (!Enum.TryParse<SortDirection>(queryValue.First(), true, out var sortDirection))
            {
                isValid = false;
                var message = "Unrecognized sort direction value";
                AddModelError(bindingContext.ModelState, SortModelBindingErrors.UnrecognizedSortValue, entityType, queryKey, message);
                continue;
            }

            if (!sortOptions.TryAddSort(propertyPath, sortDirection))
            {
                isValid = false;
                var message = $"Property does not exist on entity '{entityType.Name}'";
                AddModelError(bindingContext.ModelState, SortModelBindingErrors.UnrecognizedProperty, entityType, queryKey, message);
                continue;
            }
        }

        if (isValid && sortOptions.Any())
            bindingContext.Result = ModelBindingResult.Success(sortOptions);
        else
            bindingContext.Result = ModelBindingResult.Failed();

        return Task.CompletedTask;
    }

    public virtual void AddModelError(
        ModelStateDictionary modelStateDictionary,
        SortModelBindingErrors errorType,
        Type entityType,
        string key,
        string message)
        => modelStateDictionary.AddModelError(key, message);
}

public enum SortModelBindingErrors
{
    MissingProperty,
    UnrecognizedProperty,
    MultipleSortingDirections,
    UnrecognizedSortValue
}
