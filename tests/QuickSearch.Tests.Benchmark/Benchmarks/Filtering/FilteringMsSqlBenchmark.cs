using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Bogus;
using QuickSearch.Benchmark.Data;
using QuickSearch.Benchmark.Data.Models;
using QuickSearch.Extensions;
using QuickSearch.Options;
using QuickSearch.Tests.Benchmark.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace QuickSearch.Benchmark.Benchmarks.Filtering;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public partial class FilteringMsSqlBenchmark
{

    private static readonly int RandomizerSeed = 12345;

    private MsSqlContainer _dbContainer = default!;

    private BenchmarksDbContextMsSql _context = default!;

    [Params(200, 1000, 2000)]
    public int UsersCount;

    [GlobalSetup]
    public async Task Setup()
    {
        Randomizer.Seed = new Random(RandomizerSeed);
        var users = DataGenerator.UsersWithAccounts(UsersCount);

        _dbContainer = new MsSqlBuilder()
            .WithName("filtering-benchmark-mssql")
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        await _dbContainer.StartAsync();

        var dbContextOptions = new DbContextOptionsBuilder<BenchmarksDbContextMsSql>()
            .UseSqlServer(_dbContainer.GetConnectionString());

        _context = new BenchmarksDbContextMsSql(dbContextOptions.Options);
        await _context.Database.MigrateAsync();
        _context.Users.AddRange(users);
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    /*
     *  LINQ
     */

    [Benchmark(Baseline = true), BenchmarkCategory(Categories.Filter.EqualString)]
    public List<User> MsSql_Linq_Where_String()
    {
        return _context.Users
            .Where(u => u.FirstName == "Adrian")
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.MultipleStrings)]
    public List<User> MsSql_Linq_Where_MultipleStrings()
    {
        return _context.Users
            .Where(u => u.FirstName == "Adrian" && u.LastName == "Roberts" && u.Email == "Adrian69@gmail.com")
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.ContainsString)]
    public List<User> MsSql_Linq_Where_ContainsString()
    {
        return _context.Users
            .Where(u => u.Email.Contains("Adrian"))
            .ToList();
    }

    /*
     *  QuickSearch
     */

    [Benchmark, BenchmarkCategory(Categories.Filter.EqualString)]
    public List<User> MsSql_QuickSearch_Filter_String()
    {
        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Eq, "Adrian");

        return _context.Users
            .Filtered(filter)
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.MultipleStrings)]
    public List<User> MsSql_QuickSearch_Filter_MultipleStrings()
    {
        var filter = new FilterOptions<User>()
            .AddFilter(u => u.FirstName, FilterType.Eq, "Adrian")
            .AddFilter(u => u.LastName, FilterType.Eq, "Roberts")
            .AddFilter(u => u.Email, FilterType.Eq, "Adrian69@gmail.com");

        return _context.Users
            .Filtered(filter)
            .ToList();
    }

    [Benchmark, BenchmarkCategory(Categories.Filter.ContainsString)]
    public List<User> MsSql_QuickSearch_Filter_ContainsString()
    {
        return _context.Users
            .Where(u => u.Email.Contains("Adrian"))
            .ToList();
    }
}
