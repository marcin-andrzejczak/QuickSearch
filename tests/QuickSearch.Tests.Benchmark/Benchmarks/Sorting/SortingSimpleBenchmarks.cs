using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Bogus;
using QuickSearch.Benchmark.Benchmarks.Filtering;
using QuickSearch.Benchmark.Data.Models;
using QuickSearch.Sort;
using QuickSearch.Tests.Benchmark.Data;

namespace QuickSearch.Benchmark.Benchmarks.Sorting;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class SortingSimpleBenchmarks
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

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.Sort.ByString)]
    public List<User> Linq_OrderBy_String()
    {
        return _usersQuery
            .OrderBy(u => u.FirstName)
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Sort.ByMultipleStrings)]
    public List<User> Linq_OrderBy_MultipleStrings()
    {
        return _usersQuery
            .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ThenBy(u => u.JobTitle)
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Sort.ByString)]
    public List<User> QuickSearch_Sort_String()
    {
        var sort = new SortOptions<User>()
            .AddSort(u => u.FirstName, SortDirection.Asc);

        return _usersQuery
            .Sorted(sort)
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Sort.ByMultipleStrings)]
    public List<User> QuickSearch_Sort_MultipleStrings()
    {
        var sort = new SortOptions<User>()
            .AddSort(u => u.FirstName, SortDirection.Asc)
            .AddSort(u => u.LastName, SortDirection.Asc)
            .AddSort(u => u.JobTitle, SortDirection.Asc);

        return _usersQuery
            .Sorted(sort)
            .ToList();
    }
}
