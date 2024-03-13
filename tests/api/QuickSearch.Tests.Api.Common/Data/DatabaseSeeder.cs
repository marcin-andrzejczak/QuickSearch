using Bogus;
using QuickSearch.Tests.Api.Common.Data.Generators;
using QuickSearch.Tests.Api.Common.Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace QuickSearch.Tests.Api.Common.Data;

public static class DatabaseSeeder
{
    private static readonly int RandomizerSeed = 12345;

    public static void SeedDatabase<TDbContext>(IApplicationBuilder app)
        where TDbContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        Randomizer.Seed = new Random(RandomizerSeed);

        var users = UserGenerators.Base.Generate(200);

        db.AddRange(users);

        db.SaveChanges();
    }
}
