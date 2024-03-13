using Bogus;
using QuickSearch.Options;
using QuickSearch.Query;

namespace QuickSearch.Tests.Unit.Query;

public class PaginationQueryBuilderTests
{
    internal class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private readonly Faker _faker;

    public PaginationQueryBuilderTests()
    {
        Randomizer.Seed = new Random(12345);

        _faker = new Faker();
    }


    #region Page

    [Fact]
    public void Page_ValidData_SetsPrefixAndOptions()
    {
        // Arrange
        var prefix = "pageoptionseasyprefix";
        var options = new PageOptions();
        var sut = new PaginationQueryBuilder<User>();

        // Act
        sut.Page(prefix, options);

        // Assert
        Assert.Equal(prefix, sut.PagePrefix);
        Assert.StrictEqual(options, sut.PageOptions);
    }

    [Fact]
    public void Page_MissingPrefix_ThrowsArgumentNullException()
    {
        // Arrange
        var expectedParamName = "prefix";
        var expectedErrorMessage = $"Prefix is required (Parameter '{expectedParamName}')";

        var options = new PageOptions();
        var sut = new PaginationQueryBuilder<User>();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => sut.Page(null!, options)
        );

        // Assert
        Assert.Equal(expectedErrorMessage, exception.Message);
        Assert.Equal(expectedParamName, exception.ParamName);
    }

    [Fact]
    public void Page_MissingOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var expectedParamName = "page";
        var expectedErrorMessage = $"Options are required (Parameter '{expectedParamName}')";

        var prefix = "p";
        var sut = new PaginationQueryBuilder<User>();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => sut.Page(prefix, null!)
        );

        // Assert
        Assert.Equal(expectedErrorMessage, exception.Message);
        Assert.Equal(expectedParamName, exception.ParamName);
    }

    #endregion Page

    #region Filter

    [Fact]
    public void Filter_ValidData_SetsPrefixAndOptions()
    {
        // Arrange
        var prefix = "filteroptionseasyprefix";
        var options = new FilterOptions<User>();
        var sut = new PaginationQueryBuilder<User>();

        // Act
        sut.Filter(prefix, options);

        // Assert
        Assert.Equal(prefix, sut.FilterPrefix);
        Assert.StrictEqual(options, sut.FilterOptions);
    }

    [Fact]
    public void Filter_MissingPrefix_ThrowsArgumentNullException()
    {
        // Arrange
        var expectedParamName = "prefix";
        var expectedErrorMessage = $"Prefix is required (Parameter '{expectedParamName}')";

        var options = new FilterOptions<User>();
        var sut = new PaginationQueryBuilder<User>();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => sut.Filter(null!, options)
        );

        // Assert
        Assert.Equal(expectedErrorMessage, exception.Message);
        Assert.Equal(expectedParamName, exception.ParamName);
    }

    [Fact]
    public void Filter_MissingOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var expectedParamName = "filter";
        var expectedErrorMessage = $"Options are required (Parameter '{expectedParamName}')";

        var prefix = "f";
        var sut = new PaginationQueryBuilder<User>();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => sut.Filter(prefix, null!)
        );

        // Assert
        Assert.Equal(expectedErrorMessage, exception.Message);
        Assert.Equal(expectedParamName, exception.ParamName);
    }

    #endregion Filter

    #region Sort

    [Fact]
    public void Sort_ValidData_SetsPrefixAndOptions()
    {
        // Arrange
        var prefix = "sortoptionseasyprefix";
        var options = new SortOptions<User>();
        var sut = new PaginationQueryBuilder<User>();

        // Act
        sut.Sort(prefix, options);

        // Assert
        Assert.Equal(prefix, sut.SortPrefix);
        Assert.StrictEqual(options, sut.SortOptions);
    }

    [Fact]
    public void Sort_MissingPrefix_ThrowsArgumentNullException()
    {
        // Arrange
        var expectedParamName = "prefix";
        var expectedErrorMessage = $"Prefix is required (Parameter '{expectedParamName}')";

        var options = new SortOptions<User>();
        var sut = new PaginationQueryBuilder<User>();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => sut.Sort(null!, options)
        );

        // Assert
        Assert.Equal(expectedErrorMessage, exception.Message);
        Assert.Equal(expectedParamName, exception.ParamName);
    }

    [Fact]
    public void Sort_MissingOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var expectedParamName = "sort";
        var expectedErrorMessage = $"Options are required (Parameter '{expectedParamName}')";

        var prefix = "s";
        var sut = new PaginationQueryBuilder<User>();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => sut.Sort(prefix, null!)
        );

        // Assert
        Assert.Equal(expectedErrorMessage, exception.Message);
        Assert.Equal(expectedParamName, exception.ParamName);
    }

    #endregion Sort

    #region ToQueryString

    [Fact]
    public void ToQueryString_Page_ReturnsQueryString()
    {
        // Arrange
        var prefix = "p";
        var pageNumber = _faker.Random.Int();
        var pageSize = _faker.Random.Int();
        var sut = new PaginationQueryBuilder<User>()
            .Page(prefix, new PageOptions
            {
                Number = pageNumber,
                Size = pageSize
            });

        var expectedUrl = $"{prefix}.Number={pageNumber}&{prefix}.Size={pageSize}";

        // Act
        var queryString = sut.ToQueryString();

        // Assert
        Assert.Equal(expectedUrl, queryString);
    }

    [Fact]
    public void ToQueryString_Filter_ReturnsQueryString()
    {
        // Arrange
        var prefix = "f";
        var idValue = _faker.Random.Int();
        var nameValue = _faker.Person.FirstName;
        var sut = new PaginationQueryBuilder<User>()
            .Filter(prefix, new FilterOptions<User>()
                .AddFilter(u => u.Id, FilterType.Eq, idValue)
                .AddFilter(u => u.Name, FilterType.Like, nameValue)
            );

        var expectedUrl = $"{prefix}.Id.Eq={idValue}&{prefix}.Name.Like={nameValue}";

        // Act
        var queryString = sut.ToQueryString();

        // Assert
        Assert.Equal(expectedUrl, queryString);
    }

    [Fact]
    public void ToQueryString_Sort_ReturnsQueryString()
    {
        // Arrange
        var prefix = "s";
        var sut = new PaginationQueryBuilder<User>()
            .Sort(prefix, new SortOptions<User>()
                .AddSort(u => u.Id, SortDirection.Desc)
                .AddSort(u => u.Name, SortDirection.Asc)
            );

        var expectedUrl = $"{prefix}.Id=Desc&{prefix}.Name=Asc";

        // Act
        var queryString = sut.ToQueryString();

        // Assert
        Assert.Equal(expectedUrl, queryString);
    }

    [Fact]
    public void ToQueryString_PageFilter_ReturnsQueryString()
    {
        // Arrange
        var pagePrefix = "p";
        var filterPrefix = "f";
        var pageNumber = _faker.Random.Int();
        var pageSize = _faker.Random.Int();
        var filterIdValue = _faker.Random.Int();
        var filterNameValue = _faker.Person.FirstName;
        var sut = new PaginationQueryBuilder<User>()
            .Page(pagePrefix, new PageOptions
            {
                Number = pageNumber,
                Size = pageSize
            })
            .Filter(filterPrefix, new FilterOptions<User>()
                .AddFilter(u => u.Id, FilterType.Eq, filterIdValue)
                .AddFilter(u => u.Name, FilterType.Like, filterNameValue)
            );

        var expectedUrl = $"{pagePrefix}.Number={pageNumber}&{pagePrefix}.Size={pageSize}" +
                          $"&" +
                          $"{filterPrefix}.Id.Eq={filterIdValue}&{filterPrefix}.Name.Like={filterNameValue}";

        // Act
        var queryString = sut.ToQueryString();

        // Assert
        Assert.Equal(expectedUrl, queryString);
    }

    [Fact]
    public void ToQueryString_PageSort_ReturnsQueryString()
    {
        // Arrange
        var pagePrefix = "p";
        var sortPrefix = "s";
        var pageNumber = _faker.Random.Int();
        var pageSize = _faker.Random.Int();
        var filterIdValue = _faker.Random.Int();
        var filterNameValue = _faker.Person.FirstName;
        var sut = new PaginationQueryBuilder<User>()
            .Page(pagePrefix, new PageOptions
            {
                Number = pageNumber,
                Size = pageSize
            })
            .Sort(sortPrefix, new SortOptions<User>()
                .AddSort(u => u.Id, SortDirection.Desc)
                .AddSort(u => u.Name, SortDirection.Asc)
            );

        var expectedUrl = $"{pagePrefix}.Number={pageNumber}&{pagePrefix}.Size={pageSize}" +
                          $"&" +
                          $"{sortPrefix}.Id=Desc&{sortPrefix}.Name=Asc";

        // Act
        var queryString = sut.ToQueryString();

        // Assert
        Assert.Equal(expectedUrl, queryString);
    }

    [Fact]
    public void ToQueryString_FilterSort_ReturnsQueryString()
    {
        // Arrange
        var sortPrefix = "s";
        var filterPrefix = "f";
        var filterIdValue = _faker.Random.Int();
        var filterNameValue = _faker.Person.FirstName;
        var sut = new PaginationQueryBuilder<User>()
            .Filter(filterPrefix, new FilterOptions<User>()
                .AddFilter(u => u.Id, FilterType.Eq, filterIdValue)
                .AddFilter(u => u.Name, FilterType.Like, filterNameValue)
            )
            .Sort(sortPrefix, new SortOptions<User>()
                .AddSort(u => u.Id, SortDirection.Desc)
                .AddSort(u => u.Name, SortDirection.Asc)
            );

        var expectedUrl = $"{filterPrefix}.Id.Eq={filterIdValue}&{filterPrefix}.Name.Like={filterNameValue}" +
                          $"&" +
                          $"{sortPrefix}.Id=Desc&{sortPrefix}.Name=Asc";

        // Act
        var queryString = sut.ToQueryString();

        // Assert
        Assert.Equal(expectedUrl, queryString);
    }

    [Fact]
    public void ToQueryString_PageFilterSort_ReturnsQueryString()
    {
        // Arrange
        var pagePrefix = "p";
        var sortPrefix = "s";
        var filterPrefix = "f";
        var pageNumber = _faker.Random.Int();
        var pageSize = _faker.Random.Int();
        var filterIdValue = _faker.Random.Int();
        var filterNameValue = _faker.Person.FirstName;
        var sut = new PaginationQueryBuilder<User>()
            .Page(pagePrefix, new PageOptions
            {
                Number = pageNumber,
                Size = pageSize
            })
            .Filter(filterPrefix, new FilterOptions<User>()
                .AddFilter(u => u.Id, FilterType.Eq, filterIdValue)
                .AddFilter(u => u.Name, FilterType.Like, filterNameValue)
            )
            .Sort(sortPrefix, new SortOptions<User>()
                .AddSort(u => u.Id, SortDirection.Desc)
                .AddSort(u => u.Name, SortDirection.Asc)
            );

        var expectedUrl = $"{pagePrefix}.Number={pageNumber}&{pagePrefix}.Size={pageSize}" +
                          $"&" +
                          $"{filterPrefix}.Id.Eq={filterIdValue}&{filterPrefix}.Name.Like={filterNameValue}" +
                          $"&" +
                          $"{sortPrefix}.Id=Desc&{sortPrefix}.Name=Asc";

        // Act
        var queryString = sut.ToQueryString();

        // Assert
        Assert.Equal(expectedUrl, queryString);
    }

    #endregion ToQueryString

}
