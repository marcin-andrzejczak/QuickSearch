using Bogus;
using QuickSearch.Extensions;
using QuickSearch.Options;

namespace QuickSearch.Tests.Unit.Extensions;

public class QueryableSortingExtensionsTests
{
    internal class Account
    {
        public int? BalanceNullable { get; set; }
        public int BalanceNonNullable { get; set; }
    }

    internal class User
    {
        public string? FirstName { get; set; }
        public Account? Account { get; set; }
    }

    private readonly IQueryable<User> _users;
    private readonly Faker _faker;

    public QueryableSortingExtensionsTests()
    {
        Randomizer.Seed = new Random(12345);
        var accountFaker = new Faker<Account>()
            .RuleFor(a => a.BalanceNullable, f => f.Random.Int());

        _faker = new Faker();
        _users = new Faker<User>()
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .RuleFor(u => u.Account, _ => accountFaker.Generate())
            .Generate(100)
            .AsQueryable();
    }

    #region Sorted

    [Fact]
    public void Sorted_FirstAscThenDesc_ReturnsOrdered()
    {
        // Arrange
        var expectedOrderedUsers = _users
            .OrderBy(u => u.FirstName)
            .ThenByDescending(u => u.Account!.BalanceNullable)
            .ToList();

        var sortOptions = new SortOptions<User>()
            .AddSort(u => u.FirstName, SortDirection.Asc)
            .AddSort(u => u.Account!.BalanceNullable, SortDirection.Desc);

        // Act
        var result = _users.Sorted(sortOptions).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedOrderedUsers.Count, result.Count);
        Assert.All(expectedOrderedUsers,
            (user, index) => Assert.StrictEqual(user, result[index])
        );
    }

    [Fact]
    public void Sorted_FirstDescThenAsc_ReturnsOrdered()
    {
        // Arrange
        var expectedOrderedUsers = _users
            .OrderByDescending(u => u.FirstName)
            .ThenBy(u => u.Account!.BalanceNullable)
            .ToList();

        var sortOptions = new SortOptions<User>()
            .AddSort(u => u.FirstName, SortDirection.Desc)
            .AddSort(u => u.Account!.BalanceNullable, SortDirection.Asc);

        // Act
        var result = _users.Sorted(sortOptions).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedOrderedUsers.Count, result.Count);
        Assert.All(expectedOrderedUsers,
            (user, index) => Assert.StrictEqual(user, result[index])
        );
    }

    [Fact]
    public void Sorted_NullSortOptionsPassed_ReturnsUnchangedOrderedQuery()
    {
        // Act
        var result = _users.Sorted(null);

        // Assert
        Assert.NotEmpty(result);
        Assert.StrictEqual(_users, result);
        Assert.Equal(_users.Count(), result.Count());
        Assert.All(_users.ToList(),
            u => Assert.Contains(u, result)
        );
    }

    [Fact]
    public void Sorted_NoneSorterPassed_ReturnsUnchangedOrderedQuery()
    {
        // Arrange
        var sortOptions = new SortOptions<User>()
            .AddSort(u => u.FirstName, SortDirection.None)
            .AddSort(u => u.Account!.BalanceNullable, SortDirection.None);

        // Act
        var result = _users.Sorted(sortOptions);

        // Assert
        Assert.NotEmpty(result);
        Assert.StrictEqual(_users, result);
        Assert.Equal(_users.Count(), result.Count());
        Assert.All(_users.ToList(),
            u => Assert.Contains(u, result)
        );
    }

    #endregion Sorted
}
