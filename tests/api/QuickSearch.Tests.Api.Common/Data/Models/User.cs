using System.ComponentModel.DataAnnotations;

namespace QuickSearch.Tests.Api.Common.Data.Models;

public class User : IEquatable<User>
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

    public bool Equals(User? other)
        => Id == other?.Id
        && Account == other?.Account
        && FirstName == other?.FirstName
        && LastName == other?.LastName
        && FullName == other?.FullName
        && PhoneNumber == other?.PhoneNumber
        && Email == other?.Email
        && CompanyName == other?.CompanyName
        && JobTitle == other?.JobTitle;

    public override bool Equals(object? obj)
        => Equals(obj as User);

    public static bool operator ==(User? left, User? right)
    {
        if (left is null || right is null)
            return Equals(left, right);

        return left.Equals(right);
    }

    public static bool operator !=(User? left, User? right)
    {
        if (left is null || right is null)
            return !Equals(left, right);

        return !left.Equals(right);
    }

    public override int GetHashCode()
        => GetHashCode();
}
