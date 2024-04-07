using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;

namespace QuickSearch.Extensions.Swashbuckle.Extensions;

internal static class SwaggerUIOptionsExtensions
{
    internal static void InjectJavascript(this SwaggerUIOptions options, string[] paths)
    {
        var contents = paths
            .Select(p => ReadEmbeddedScript(p))
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();

        if (!contents.Any())
            return;

        var builder = new StringBuilder(options.HeadContent);
        builder.AppendLine("<script>");

        foreach (var content in contents)
        {
            builder.AppendLine();
            builder.Append(content);
            builder.AppendLine();
        }

        builder.AppendLine("</script>");

        options.HeadContent = builder.ToString();
    }

    private static string? ReadEmbeddedScript(string path)
    {
        using var contentStream = typeof(SwaggerUIOptionsExtensions)
            .GetTypeInfo().Assembly
            .GetManifestResourceStream(path);

        if (contentStream is null)
            return null;

        using var contentReader = new StreamReader(contentStream);
        return contentReader.ReadToEnd();
    }
}
