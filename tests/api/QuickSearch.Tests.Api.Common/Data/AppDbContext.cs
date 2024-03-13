using QuickSearch.Tests.Api.Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace QuickSearch.Tests.Api.Common.Data;

public class AppDbContext : DbContext
{
    public virtual DbSet<User> Users { get; set; } = null!;
    public virtual DbSet<Account> Accounts { get; set; } = null!;

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
}
