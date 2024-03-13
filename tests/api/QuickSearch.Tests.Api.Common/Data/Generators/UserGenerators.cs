using Bogus;
using QuickSearch.Tests.Api.Common.Data.Models;

namespace QuickSearch.Tests.Api.Common.Data.Generators;

public static class UserGenerators
{
    public static Faker<User> Base => new Faker<User>()
        .RuleFor(u => u.Id, f => f.Random.Guid())
        .RuleFor(u => u.Account, _ => AccountGenerators.Base)
        .RuleFor(u => u.FirstName, f => f.Person.FirstName)
        .RuleFor(u => u.LastName, f => f.Person.LastName)
        .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
        .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
        .RuleFor(u => u.CompanyName, f => f.Company.CompanyName())
        .RuleFor(u => u.JobTitle, f => f.Name.JobTitle());
}