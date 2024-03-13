using QuickSearch.Pagination;

namespace QuickSearch.Tests.Unit.Options;

public class PageOptionsTests
{
    internal class User
    {
        public string? FirstName { get; set; }
        public string LastName { get; set; } = default!;
    }

    #region ToQueryString

    [Fact]
    public void ToQueryString_ValidOptions_ReturnsUrlEncodedQueryString()
    {
        // Arrange
        var prefix = "s";
        var pageNumber = 10;
        var pageSize = 40;
        var expectedQueryString = $"{prefix}.Number={pageNumber}&{prefix}.Size={pageSize}";
        var pageOptions = new PageOptions
        {
            Number = pageNumber,
            Size = pageSize,
        };

        // Act
        var queryString = pageOptions.ToQueryString(prefix);

        // Assert
        Assert.Equal(expectedQueryString, queryString);
    }

    #endregion ToQueryString
}
