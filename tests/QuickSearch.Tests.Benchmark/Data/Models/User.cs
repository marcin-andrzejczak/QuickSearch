using System.ComponentModel.DataAnnotations;

namespace QuickSearch.Benchmark.Data.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public Account? Account { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName => $"{FirstName} {LastName}";
    public string PhoneNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string CompanyName { get; set; } = default!;
    public string JobTitle { get; set; } = default!;

}
