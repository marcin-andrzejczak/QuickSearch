using QuickSearch.Binding;
using QuickSearch.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace QuickSearch.Setup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuickSearch(this IServiceCollection services)
    {
        services.AddMvcCore(options =>
        {
            options.ModelBinderProviders.Insert(0, new OptionsBindersProvider());
        });

        QuickSearchMapper.Initialize();

        return services;
    }
}
