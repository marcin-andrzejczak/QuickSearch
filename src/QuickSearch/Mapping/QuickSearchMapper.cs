using QuickSearch.Filter;
using QuickSearch.Sort;

namespace QuickSearch.Mapping;

internal class QuickSearchMapper
{
    private static object _instanceLock = new object();
    private static QuickSearchMapper? _instance;
    internal static QuickSearchMapper Instance
    {
        get
        {
            if (_instance is null)
                lock (_instanceLock)
                    _instance ??= new QuickSearchMapper();

            return _instance;
        }
    }

    private static Dictionary<(Type, Type), IPropertyMap> _propertyMaps = new();
    private static bool _isInitialized = false;

    internal static void Initialize()
    {
        var propertyMappers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => t.IsAssignableTo(typeof(IPropertyMap)) && !t.IsAbstract && t.IsClass)
            .ToList();

        foreach (var propertyMapper in propertyMappers)
        {
            var mapperGenericTypes = propertyMapper.BaseType!.GetGenericArguments();
            var sourceEntityType = mapperGenericTypes[0];
            var targetEntityType = mapperGenericTypes[1];

            var mapperInstance = Activator.CreateInstance(propertyMapper) as IPropertyMap;

            if (mapperInstance is not null)
                _propertyMaps.Add((sourceEntityType, targetEntityType), mapperInstance);
        }

        _isInitialized = true;
    }

    internal void Initialize(Dictionary<(Type, Type), IPropertyMap> propertyMaps)
    {
        _propertyMaps = propertyMaps;
        _isInitialized = true;
    }

    internal void EnsureInitialized()
    {
        if (!_isInitialized)
            throw new InvalidOperationException($"{nameof(QuickSearch)} mapping has not been initialized properly");
    }

    internal SortOptions<TOut> MapOptions<TOut, TIn>(SortOptions<TIn> source)
        where TIn : class
        where TOut : class
    {
        EnsureInitialized();

        if (!_propertyMaps!.TryGetValue((typeof(TIn), typeof(TOut)), out var propertyMap))
            throw new InvalidOperationException($"No map from type {typeof(TIn).Name} to {typeof(TOut).Name} was found");

        var target = new SortOptions<TOut>();
        foreach (var (sourcePropertyPath, sourceSorter) in source.Sorters)
        {
            var mappedTargetKey = propertyMap.ApplyMap(sourcePropertyPath, target.Parameter);
            target.AddSort(mappedTargetKey, sourceSorter);
        }

        return target;
    }

    internal FilterOptions<TOut> MapOptions<TOut, TIn>(FilterOptions<TIn> source)
        where TIn : class
        where TOut : class
    {
        EnsureInitialized();

        if (!_propertyMaps!.TryGetValue((typeof(TIn), typeof(TOut)), out var propertyMap))
            throw new InvalidOperationException($"No map from type {typeof(TIn).Name} to {typeof(TOut).Name} was found");

        var target = new FilterOptions<TOut>();
        foreach (var (sourcePropertyPath, sourceFilters) in source.Filters)
        {
            var mappedTargetKey = propertyMap.ApplyMap(sourcePropertyPath, target.Parameter);
            target.AddFilters(mappedTargetKey, sourceFilters);
        }

        return target;
    }

}
