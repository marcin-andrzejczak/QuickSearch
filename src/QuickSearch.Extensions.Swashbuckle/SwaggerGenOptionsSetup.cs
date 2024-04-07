using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using QuickSearch.Filter;
using QuickSearch.Extensions.Swashbuckle.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using Microsoft.OpenApi.Any;

namespace QuickSearch.Extensions.Swashbuckle;

public static class SwaggerGenOptionsExtensions
{
    public static void AddQuickSearch(this SwaggerGenOptions options)
    {
        options.SchemaFilter<FilterOptionsSchemaFilter>();
    }

    public static void AddQuickSearch(this SwaggerUIOptions options)
    {
        options.InjectJavascript(new[] {
            "QuickSearch.Extensions.Swashbuckle.wwwroot.swagger_ui.json-schema-filter.js"
        });
    }
}

public class FilterOptionsSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType || !context.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(FilterOptions<>)))
        {
            return;
        }

        var keyType = context.Type.GetGenericArguments()[0];
        var valueType = typeof(FilterType);

        var props = keyType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => new OpenApiString(p.Name))
            .ToList<IOpenApiAny>();

        var filters = Enum.GetNames(valueType)
            .Select(f => new OpenApiString(f))
            .ToList<IOpenApiAny>(); ;

        //var properties = new Dictionary<string, OpenApiSchema>
        //{
        //    { "properties", new OpenApiSchema {
        //        Type = "array",
        //        Items = props
        //    }},
        //    { "filters", new OpenApiSchema
        //    {
        //        Enum = filters
        //    }}
        //};

        //schema.Type = "array";
        //schema.Items = new OpenApiSchema
        //{
        //    Type = "object",
        //    Format = "quicksearchfilter",
        //    Properties = properties,

        //};
        //schema.Properties = keyType
        //    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        //    .Select(p => p.Name)
        //    .ToDictionary(
        //        name => name,
        //        name => context.SchemaGenerator.GenerateSchema(valueType, context.SchemaRepository)
        //    );
    }
}