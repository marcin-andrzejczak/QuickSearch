using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QuickSearch.Options;

public class PageOptions
{

    /// <summary>
    /// Page to fetch, indexing starting at 1
    /// </summary>
    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int Number { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    /// <example>10</example>
    [Range(1, int.MaxValue)]
    public int Size { get; set; } = 25;

    internal StringBuilder ToQueryStringBuilder(string prefix)
        => new StringBuilder()
            .Append(prefix)
            .Append('.')
            .Append(nameof(Number))
            .Append('=')
            .Append(Number)
            .Append('&')
            .Append(prefix)
            .Append('.')
            .Append(nameof(Size))
            .Append('=')
            .Append(Size);

    public string ToQueryString(string prefix)
        => ToQueryStringBuilder(prefix).ToString();

}
