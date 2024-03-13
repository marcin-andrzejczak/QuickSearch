using Bogus;
using QuickSearch.Tests.Api.Common.Data.Models;

namespace QuickSearch.Tests.Api.Common.Data.Generators;

public static class AccountGenerators
{
    public static Faker<Account> Base => new Faker<Account>()
        .RuleFor(a => a.Id, f => f.Random.Guid())
        .RuleFor(a => a.Balance, f => f.Random.Int(0, 1000))
        .RuleFor(a => a.LatestTransactions, f => f.Random.Int(0, 10));
}
