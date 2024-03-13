using QuickSearch.Tests.Api.Common.Data.Models;

namespace QuickSearch.Tests.Api.Common.Dtos;

public class UserDTO : IEquatable<UserDTO>
{
    public Guid Id { get; set; }
    public int AccountBalance { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string JobTitle { get; set;} = default!;

    public static UserDTO FromUser(User user)
        => new()
        {
            Id = user.Id,
            AccountBalance = user.Account?.Balance ?? 0,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            JobTitle = user.JobTitle
        };

    public bool Equals(UserDTO? other)
        => Id == other?.Id
        && AccountBalance == other?.AccountBalance
        && FirstName == other?.FirstName
        && LastName == other?.LastName
        && PhoneNumber == other?.PhoneNumber
        && Email == other?.Email
        && JobTitle == other?.JobTitle;

    public override bool Equals(object? obj)
        => Equals(obj as UserDTO);

    public static bool operator ==(UserDTO? left, UserDTO? right)
    {
        if (left is null || right is null)
            return Equals(left, right);

        return left.Equals(right);
    }

    public static bool operator !=(UserDTO? left, UserDTO? right)
    {
        if (left is null || right is null)
            return !Equals(left, right);

        return !left.Equals(right);
    }

    public override int GetHashCode()
        => GetHashCode();
}
