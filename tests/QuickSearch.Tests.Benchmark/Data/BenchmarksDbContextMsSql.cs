using QuickSearch.Benchmark.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace QuickSearch.Benchmark.Data;

public class BenchmarksDbContextMsSql : DbContext
{
    public virtual DbSet<User> Users { get; set; } = default!;
    public virtual DbSet<Account> Account { get; set; } = default!;

    public BenchmarksDbContextMsSql()
    { 
    }

    public BenchmarksDbContextMsSql(DbContextOptions<BenchmarksDbContextMsSql> options) : base(options)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder.UseSqlServer());
    }
}
