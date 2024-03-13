using QuickSearch.Options;

namespace QuickSearch.Tests.Unit.Options;

public class SortOptionsTests
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
        var expectedQueryString = $"{prefix}.FirstName=Desc&{prefix}.LastName=Asc";
        var sortOptions = new SortOptions<User>()
            .AddSort(u => u.FirstName, SortDirection.Desc)
            .AddSort(u => u.LastName, SortDirection.Asc);

        // Act
        var queryString = sortOptions.ToQueryString(prefix);

        // Assert
        Assert.Equal(expectedQueryString, queryString);
    }

    #endregion ToQueryString
}
