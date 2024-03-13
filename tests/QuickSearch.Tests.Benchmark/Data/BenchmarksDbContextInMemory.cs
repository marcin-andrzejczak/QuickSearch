using QuickSearch.Benchmark.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace QuickSearch.Benchmark.Data;

public class BenchmarksDbContextInMemory : DbContext
{
    public virtual DbSet<User> Users { get; set; } = default!;
    public virtual DbSet<Account> Account { get; set; } = default!;

    public BenchmarksDbContextInMemory()
    { 
    }

    public BenchmarksDbContextInMemory(DbContextOptions<BenchmarksDbContextInMemory> options) : base(options)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder.UseInMemoryDatabase("QuickSearch-benchmark-inmemory"));
    }
}
