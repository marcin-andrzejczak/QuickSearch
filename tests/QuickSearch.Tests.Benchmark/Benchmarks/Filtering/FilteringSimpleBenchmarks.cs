using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Bogus;
using QuickSearch.Benchmark.Data.Models;
using QuickSearch.Extensions;
using QuickSearch.Options;
using QuickSearch.Tests.Benchmark.Data;

namespace QuickSearch.Benchmark.Benchmarks.Filtering;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class FilteringSimpleBenchmarks
{
    private static readonly int RandomizerSeed = 12345;

    private IQueryable<User> _usersQuery = default!;

    [Params(200, 1000, 2000)]
    public int UsersCount;

    [GlobalSetup]
    public void Setup()
    {
        Randomizer.Seed = new Random(RandomizerSeed);
        var users = DataGenerator.UsersWithAccounts(UsersCount);

        _usersQuery = users.AsQueryable();
    }

    /*
     *  LINQ
     */

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.Filter.EqualString)]
    public List<User> Linq_Where_String()
    {
        return _usersQuery
            .Where(u => u.FirstName == "Adrian")
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.MultipleStrings)]
    public List<User> Linq_Where_MultipleStrings()
    {
        return _usersQuery
            .Where(u => u.FirstName == "Adrian" && u.LastName == "Roberts" && u.Email == "Adrian69@gmail.com")
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.ContainsString)]
    public List<User> Linq_Where_ContainsString()
    {
        return _usersQuery
            .Where(u => u.Email.Contains("Adrian"))
            .ToList();
    }

    /*
     *  QuickSearch
     */

    [Benchmark, BenchmarkCategory(Categories.Filter.EqualString)]
    public List<User> QuickSearch_Filter_String()
    {
        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Eq, "Adrian");

        return _usersQuery
            .Filtered(filter)
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.MultipleStrings)]
    public List<User> QuickSearch_Filter_MultipleStrings()
    {
        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Eq, "Adrian")
            .AddFilter(u => u.LastName, FilterType.Eq, "Roberts")
            .AddFilter(u => u.Email, FilterType.Eq, "Adrian69@gmail.com");

        return _usersQuery
            .Filtered(filter)
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.EqualString)]
    public List<User> QuickSearch_Filter_ContainsString()
    {
        return _usersQuery
            .Where(u => u.Email.Contains("Adrian"))
            .ToList();
    }
}
