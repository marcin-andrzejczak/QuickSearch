using Bogus;
using QuickSearch.Extensions;
using QuickSearch.Options;
using MockQueryable.NSubstitute;

namespace QuickSearch.Tests.Unit.Extensions;

public class QueryablePagingExtensionsTests
{
    internal class User
    {
        public string FirstName { get; set; } = default!;
    }

    public QueryablePagingExtensionsTests()
    {
        Randomizer.Seed = new Random(12345);
    }

    #region Paged

    [Theory]
    [InlineData(100, 1, 10, 10, 10)]
    [InlineData(100, 5, 10, 10, 10)]
    [InlineData(100, 11, 10, 0, 10)]
    [InlineData(5, 1, 10, 5, 1)]
    [InlineData(5, 2, 10, 0, 1)]
    [InlineData(12, 1, 10, 10, 2)]
    [InlineData(12, 2, 10, 2, 2)]
    public void Paged_ValidData_ReturnsPage(
        int usersCount,
        int pageNumber,
        int pageSize,
        int expectedItemsCount,
        int expectedTotalPages)
    {
        // Arrange
        var usersQuery = new Faker<User>()
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .Generate(usersCount)
            .AsQueryable();

        var pageOptions = new PageOptions
        {
            Number = pageNumber,
            Size = pageSize
        };

        // Act
        var result = usersQuery.Paged(pageOptions);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(expectedItemsCount, result.Items.Count());
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(pageNumber, result.CurrentPage);
        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Fact]
    public void Paged_NullPageOptions_ReturnsPageUsingDefault()
    {
        // Arrange
        var emptyPageOptions = new PageOptions();
        var usersCount = 50;
        var expectedPageNumber = emptyPageOptions.Number;
        var expectedPageSize = emptyPageOptions.Size;
        var expectedTotalPages = usersCount / expectedPageSize;

        var usersQuery = new Faker<User>()
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .Generate(usersCount)
            .AsQueryable();

        // Act
        var result = usersQuery.Paged(null);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(expectedPageSize, result.Items.Count());
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(expectedPageNumber, result.CurrentPage);
        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    #endregion Paged

    #region PagedAsync

    [Theory]
    [InlineData(100, 1, 10, 10, 10)]
    [InlineData(100, 5, 10, 10, 10)]
    [InlineData(100, 11, 10, 0, 10)]
    [InlineData(5, 1, 10, 5, 1)]
    [InlineData(5, 2, 10, 0, 1)]
    [InlineData(12, 1, 10, 10, 2)]
    [InlineData(12, 2, 10, 2, 2)]
    public async Task PagedAsync_ValidData_ReturnsPage(
        int usersCount,
        int pageNumber,
        int pageSize,
        int expectedItemsCount,
        int expectedTotalPages)
    {
        // Arrange
        var users = new Faker<User>()
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .Generate(usersCount)
            .BuildMock();

        var pageOptions = new PageOptions
        {
            Number = pageNumber,
            Size = pageSize
        };

        // Act
        var result = await users.PagedAsync(pageOptions);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(expectedItemsCount, result.Items.Count());
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(pageNumber, result.CurrentPage);
        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Fact]
    public async Task PagedAsync_NullPageOptions_ReturnsPageUsingDefault()
    {
        // Arrange
        var emptyPageOptions = new PageOptions();
        var usersCount = 50;
        var expectedPageNumber = emptyPageOptions.Number;
        var expectedPageSize = emptyPageOptions.Size;
        var expectedTotalPages = usersCount / expectedPageSize;

        var usersQuery = new Faker<User>()
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .Generate(usersCount)
            .BuildMock();

        // Act
        var result = await usersQuery.PagedAsync(null);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(expectedPageSize, result.Items.Count());
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(expectedPageNumber, result.CurrentPage);
        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    #endregion PagedAsync

}
