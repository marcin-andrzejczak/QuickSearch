using Bogus;
using QuickSearch.Benchmark.Data.Models;

namespace QuickSearch.Tests.Benchmark.Data;

public static class DataGenerator
{
    public static List<User> UsersWithAccounts(int usersCount)
        => new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Account, _ => new Faker<Account>()
                .RuleFor(a => a.Id, f => f.Random.Guid())
                .RuleFor(a => a.Balance, f => f.Random.Int(0, 1000))
                .RuleFor(a => a.LatestTransactions, f => f.Random.Int(0, 10)))
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .RuleFor(u => u.LastName, f => f.Person.LastName)
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.CompanyName, f => f.Company.CompanyName())
            .RuleFor(u => u.JobTitle, f => f.Name.JobTitle())
            .Generate(usersCount);
}
