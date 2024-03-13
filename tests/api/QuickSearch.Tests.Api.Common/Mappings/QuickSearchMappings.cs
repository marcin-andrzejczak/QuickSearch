using QuickSearch.Tests.Api.Common.Data.Models;
using QuickSearch.Tests.Api.Common.Dtos;
using QuickSearch.Mapping;

namespace QuickSearch.Tests.Api.Common.Mappings;

public class UserPaginationMap : AbstractPropertyMap<UserDTO, User>
{
    public UserPaginationMap()
    {
        Map(dto => dto.AccountBalance, u => u.Account!.Balance);
    }
}
