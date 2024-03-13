using Bogus;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace QuickSearch.Tests.Api.Common.Data.Models;

[ExcludeFromCodeCoverage]
public class Account : IEquatable<Account>
{
    [Key]
    public Guid Id { get; set; }
    public int Balance { get; set; }
    public int LatestTransactions { get; set; }

    public bool Equals(Account? other)
        => Id == other?.Id
        && Balance == other?.Balance
        && LatestTransactions == other?.LatestTransactions;

    public override bool Equals(object? obj)
        => Equals(obj as Account);

    public static bool operator ==(Account? left, Account? right)
    {
        if (left is null || right is null)
            return Equals(left, right);

        return left.Equals(right);
    }

    public static bool operator !=(Account? left, Account? right)
    {
        if (left is null || right is null)
            return !Equals(left, right);

        return !left.Equals(right);
    }

    public override int GetHashCode()
        => GetHashCode();
}
