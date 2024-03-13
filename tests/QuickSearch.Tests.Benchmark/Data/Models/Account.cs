using System.ComponentModel.DataAnnotations;

namespace QuickSearch.Benchmark.Data.Models;

public class Account
{
    [Key]
    public Guid Id { get; set; }
    public int Balance { get; set; }
    public int LatestTransactions { get; set; }
}
