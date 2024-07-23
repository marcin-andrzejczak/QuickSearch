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

    //[Fact]
    //public void ToQueryString_MultipleFiltersForTheSameProperty_ReturnsUrlEncodedQueryString()
    //{
    //    // Arrange
    //    var prefix = "f";
    //    var firstNot = "Test";
    //    var secondNot = "Fact";
    //    var expectedQueryString = $"{prefix}.FirstName.Neq={firstNot}&{prefix}.FirstName.Neq={secondNot}";
    //    var filterOptions = new FilterOptions<User>()
    //        .AddFilter(u => u.FirstName, FilterType.Neq, firstNot)
    //        .AddFilter(u => u.FirstName, FilterType.Neq, secondNot);

    //    // Act
    //    var queryString = filterOptions.ToQueryString(prefix);

    //    // Assert
    //    Assert.Equal(expectedQueryString, queryString);
    //}

    #endregion ToQueryString
}
