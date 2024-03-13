using Bogus;
using QuickSearch.Extensions;
using QuickSearch.Options;

namespace QuickSearch.Tests.Unit.Extensions;

public class QueryableFilteringExtensionsTests
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

    public QueryableFilteringExtensionsTests()
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

    #region Helpers

    #endregion Helpers

    public string GetRandomName()
    {
        var names = _users.Select(u => u.FirstName).ToList();
        var randomNameIndex = new Random().Next(0, names.Count - 1);
        return names[randomNameIndex]!;
    }

    public int GetRandomBalance()
    {
        var balances = _users.Select(u => u.Account!.BalanceNullable).ToList();
        var randomBalanceIndex = new Random().Next(0, balances.Count - 1);
        return balances[randomBalanceIndex]!.Value;
    }

    #region Filtered

    [Fact]
    public void Filtered_ValidNonNullableFilterPassed_ReturnsFilteredQueryable()
    {
        // Arrange
        var name = GetRandomName();
        var balance = GetRandomBalance();

        var expectedUsers = _users
            .Where(u => u.FirstName == name)
            .ToList();

        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Eq, name);

        // Act
        var result = _users.Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedUsers.Count, result.Count);
        Assert.All(expectedUsers,
            u => Assert.Contains(u, result)
        );
    }

    [Fact]
    public void Filtered_ValidNullableFilterPassed_ReturnsFilteredQueryable()
    {
        // Arrange
        var name = GetRandomName();
        var balance = GetRandomBalance();

        var expectedUsers = _users
            .Where(u => u.Account!.BalanceNullable == balance)
            .ToList();

        var filter = new FilterOptions<User>()
            .AddFilter(u => u.Account!.BalanceNullable, FilterType.Eq, balance);

        // Act
        var result = _users.Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedUsers.Count, result.Count);
        Assert.All(expectedUsers,
            u => Assert.Contains(u, result)
        );
    }

    [Fact]
    public void Filtered_NullFilterOptionsPassed_ReturnsUnchangedQuery()
    {
        // Act
        var result = _users.Filtered(null);

        // Assert
        Assert.NotEmpty(result);
        Assert.StrictEqual(_users, result);
        Assert.Equal(_users.Count(), result.Count());
        Assert.All(_users.ToList(),
            u => Assert.Contains(u, result)
        );
    }

    [Fact]
    public void Filtered_FilterHasNoRegisteredComparer_ReturnsUnchangedQuery()
    {
        // Arrange
        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.None, _faker.Random.Guid().ToString());

        // Act
        var result = _users.Filtered(filter);

        // Assert
        Assert.NotEmpty(result);
        Assert.StrictEqual(_users, result);
        Assert.Equal(_users.Count(), result.Count());
        Assert.All(_users.ToList(),
            u => Assert.Contains(u, result)
        );
    }

    [Fact]
    public void Filtered_NullFilterValuePassed_FiltersAccordingly()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { BalanceNullable = 1 },
            new Account { BalanceNullable = null },
        };

        var filter = new FilterOptions<Account>()
            .AddFilter(u => u.BalanceNullable, FilterType.Eq, null);

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.All(result.ToList(),
            a => Assert.Contains(a, accounts)
        );
    }

    [Fact]
    public void Filtered_MultipleDifferentFilterTypesForProperty_ConstructsAndExpressionForThem()
    {
        // Arrange
        var accountsCreated = 0;
        var accounts = new Faker<Account>()
            .RuleFor(a => a.BalanceNonNullable, _ => ++accountsCreated)
            .Generate(100);

        var filter = new FilterOptions<Account>()
            .AddFilter(u => u.BalanceNonNullable, FilterType.Gt, 49)
            .AddFilter(u => u.BalanceNonNullable, FilterType.Lt, 51);

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.Equal(50, result.First().BalanceNonNullable);
        Assert.All(result.ToList(),
            a => Assert.Contains(a, accounts)
        );
    }

    [Fact]
    public void Filtered_MultipleSameFilterTypesForProperty_ConstructsOrExpressionForThem()
    {
        // Arrange
        var accountsCreated = 0;
        var accounts = new Faker<Account>()
            .RuleFor(a => a.BalanceNonNullable, _ => ++accountsCreated)
            .Generate(100);

        var filter = new FilterOptions<Account>()
            .AddFilter(u => u.BalanceNonNullable, FilterType.Eq, 49)
            .AddFilter(u => u.BalanceNonNullable, FilterType.Eq, 51);

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(49, result[0].BalanceNonNullable);
        Assert.Equal(51, result[1].BalanceNonNullable);
        Assert.All(result.ToList(),
            a => Assert.Contains(a, accounts)
        );
    }

    #region FilterTypes

    [Fact]
    public void Filtered_FilterType_Lt_ReturnsFiltered()
    {
        // Arrange
        var accountsCreated = 0;
        var accounts = new Faker<Account>()
            .RuleFor(a => a.BalanceNonNullable, _ => ++accountsCreated)
            .Generate(100);

        var filterValue = 50;
        var expectedCount = filterValue - 1;
        var filter = new FilterOptions<Account>()
            .AddFilter(u => u.BalanceNonNullable, FilterType.Lt, filterValue);

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result.ToList(),
            a => Assert.True(a.BalanceNonNullable < filterValue)
        );
    }

    [Fact]
    public void Filtered_FilterType_Lte_ReturnsFiltered()
    {
        // Arrange
        var accountsCreated = 0;
        var accounts = new Faker<Account>()
            .RuleFor(a => a.BalanceNonNullable, _ => ++accountsCreated)
            .Generate(100);

        var filterValue = 50;
        var expectedCount = filterValue;
        var filter = new FilterOptions<Account>()
            .AddFilter(u => u.BalanceNonNullable, FilterType.Lte, filterValue);

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result.ToList(),
            a => Assert.True(a.BalanceNonNullable <= filterValue)
        );
    }

    [Fact]
    public void Filtered_FilterType_Gt_ReturnsFiltered()
    {
        // Arrange
        var accountsCreated = 0;
        var accounts = new Faker<Account>()
            .RuleFor(a => a.BalanceNonNullable, _ => ++accountsCreated)
            .Generate(100);

        var filterValue = 50;
        var expectedCount = accountsCreated - filterValue;
        var filter = new FilterOptions<Account>()
            .AddFilter(u => u.BalanceNonNullable, FilterType.Gt, filterValue);

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result.ToList(),
            a => Assert.True(a.BalanceNonNullable > filterValue)
        );
    }

    [Fact]
    public void Filtered_FilterType_Gte_ReturnsFiltered()
    {
        // Arrange
        var accountsCreated = 0;
        var accounts = new Faker<Account>()
            .RuleFor(a => a.BalanceNonNullable, _ => ++accountsCreated)
            .Generate(100);

        var filterValue = 50;
        var expectedCount = accountsCreated - filterValue + 1;
        var filter = new FilterOptions<Account>()
            .AddFilter(u => u.BalanceNonNullable, FilterType.Gte, filterValue);

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result.ToList(),
            a => Assert.True(a.BalanceNonNullable >= filterValue)
        );
    }

    [Fact]
    public void Filtered_FilterType_Eq_ReturnsFiltered()
    {
        // Arrange
        var accountsCreated = 0;
        var accounts = new Faker<Account>()
            .RuleFor(a => a.BalanceNonNullable, _ => ++accountsCreated)
            .Generate(100);

        var filterValue = 50;
        var filter = new FilterOptions<Account>()
            .AddFilter(u => u.BalanceNonNullable, FilterType.Eq, filterValue);

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.All(result.ToList(),
            a => Assert.True(a.BalanceNonNullable == filterValue)
        );
    }

    [Fact]
    public void Filtered_FilterType_Neq_ReturnsFiltered()
    {
        // Arrange
        var accountsCreated = 0;
        var accounts = new Faker<Account>()
            .RuleFor(a => a.BalanceNonNullable, _ => ++accountsCreated)
            .Generate(100);

        var expectedCount = accountsCreated - 1;
        var filterValue = 50;
        var filter = new FilterOptions<Account>()
            .AddFilter(u => u.BalanceNonNullable, FilterType.Neq, filterValue);

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result.ToList(),
            a => Assert.True(a.BalanceNonNullable != filterValue)
        );
    }

    [Fact]
    public void Filtered_FilterType_Like_ReturnsFiltered()
    {
        // Arrange
        var nameBase = "name";
        var accountsCreated = 0;
        var accounts = new Faker<User>()
            .RuleFor(a => a.FirstName, _ => $"{nameBase}{++accountsCreated}")
            .Generate(100);

        var filterValue = 50;
        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Like, filterValue.ToString());

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.All(result.ToList(),
            a => Assert.True(a.FirstName == $"{nameBase}{filterValue}")
        );
    }

    [Fact]
    public void Filtered_FilterType_Nlike_ReturnsFiltered()
    {
        // Arrange
        var nameBase = "name";
        var accountsCreated = 0;
        var accounts = new Faker<User>()
            .RuleFor(a => a.FirstName, _ => $"{nameBase}{++accountsCreated}")
            .Generate(100);

        var expectedCount = accountsCreated - 1;
        var filterValue = 50;
        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Nlike, filterValue.ToString());

        // Act
        var result = accounts.AsQueryable().Filtered(filter).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result.ToList(),
            a => Assert.True(a.FirstName != $"{nameBase}{filterValue}")
        );
    }

    #endregion FilterTypes

    #endregion Filtered
}
