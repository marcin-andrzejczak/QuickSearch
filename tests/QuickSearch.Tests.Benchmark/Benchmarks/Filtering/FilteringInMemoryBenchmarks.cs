using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Bogus;
using QuickSearch.Benchmark.Data;
using QuickSearch.Benchmark.Data.Models;
using QuickSearch.Tests.Benchmark.Data;
using Microsoft.EntityFrameworkCore;
using QuickSearch.Filter;

namespace QuickSearch.Benchmark.Benchmarks.Filtering;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class FilteringInMemoryBenchmarks
{
    private static readonly int RandomizerSeed = 12345;

    private BenchmarksDbContextInMemory _context = default!;

    [Params(200, 1000, 2000)]
    public int UsersCount;

    [GlobalSetup]
    public void Setup()
    {
        Randomizer.Seed = new Random(RandomizerSeed);
        var users = DataGenerator.UsersWithAccounts(UsersCount);

        var dbContextOptions = new DbContextOptionsBuilder<BenchmarksDbContextInMemory>()
            .UseInMemoryDatabase(nameof(FilteringInMemoryBenchmarks));

        _context = new BenchmarksDbContextInMemory(dbContextOptions.Options);
        _context.Users.AddRange(users);
    }

    /*
     *  LINQ
     */

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.Filter.EqualString)]
    public List<User> InMemory_Linq_Where_String()
    {
        return _context.Users
            .Where(u => u.FirstName == "Adrian")
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.MultipleStrings)]
    public List<User> InMemory_Linq_Where_MultipleStrings()
    {
        return _context.Users
            .Where(u => u.FirstName == "Adrian" && u.LastName == "Roberts" && u.Email == "Adrian69@gmail.com")
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.ContainsString)]
    public List<User> InMemory_Linq_Where_ContainsString()
    {
        return _context.Users
            .Where(u => u.Email.Contains("Adrian"))
            .ToList();
    }

    /*
     *  QuickSearch
     */

    [Benchmark, BenchmarkCategory(Categories.Filter.EqualString)]
    public List<User> InMemory_QuickSearch_Filter_String()
    {
        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Eq, "Adrian");

        return _context.Users
            .Filter(filter)
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.MultipleStrings)]
    public List<User> InMemory_QuickSearch_Filter_MultipleStrings()
    {
        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Eq, "Adrian")
            .AddFilter(u => u.LastName, FilterType.Eq, "Roberts")
            .AddFilter(u => u.Email, FilterType.Eq, "Adrian69@gmail.com");

        return _context.Users
            .Filter(filter)
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.ContainsString)]
    public List<User> InMemory_QuickSearch_Filter_ContainsString()
    {
        return _context.Users
            .Where(u => u.Email.Contains("Adrian"))
            .ToList();
    }
}
