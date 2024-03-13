using QuickSearch.Filter;

namespace QuickSearch.Tests.Unit.Options;

public class FilterOptionsTests
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
        var prefix = "f";
        var firstName = "Test";
        var lastNameContains = "Testerson";
        var expectedQueryString = $"{prefix}.FirstName.Eq={firstName}&{prefix}.LastName.Like={lastNameContains}";
        var filterOptions = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Eq, firstName)
            .AddFilter(u => u.LastName, FilterType.Like, lastNameContains);

        // Act
        var queryString = filterOptions.ToQueryString(prefix);

        // Assert
        Assert.Equal(expectedQueryString, queryString);
    }

    #endregion ToQueryString
}
