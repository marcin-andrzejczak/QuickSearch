using QuickSearch.Tests.Api.Common.Data;
using QuickSearch.Tests.Api.Common.Data.Generators;
using QuickSearch.Tests.Api.Common.Data.Models;
using QuickSearch.Tests.Api.Common.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using QuickSearch.Pagination;

namespace QuickSearch.Tests.Integration.Mvc.Controllers;

public class UserControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _context;

    public UserControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _context = factory.Services.GetRequiredService<AppDbContext>();
    }

    #region GetSorted

    [Fact]
    public async Task GetSorted_ValidQuery_ReturnsSorted()
    {
        // Arrange
        var expectedList = await _context.Users
            .Include(u => u.Account)
            .OrderBy(u => u.FirstName)
            .ThenByDescending(u => u.Account!.Balance)
            .ToListAsync();

        var url = "users/sorted?s.firstName=asc&s.account.balance=desc";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseUsers = await response.Content.ReadFromJsonAsync<List<User>>();

        Assert.NotNull(responseUsers);
        Assert.NotEmpty(responseUsers);
        Assert.Equal(expectedList.Count, responseUsers.Count);
        Assert.All(responseUsers,
            u => Assert.Contains(u, expectedList)
        );
    }

    #endregion GetSorted

    #region GetFiltered

    [Fact]
    public async Task GetFiltered_ValidQuery_ReturnsFiltered()
    {
        // Arrange
        var expectedJobTitleContains = "Central";
        var expectedList = await _context.Users
            .Include(u => u.Account)
            .Where(u => u.JobTitle.Contains(expectedJobTitleContains))
            .ToListAsync();

        var url = "users/filtered?f.jobTitle.like=Central";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseUsers = await response.Content.ReadFromJsonAsync<List<User>>();

        Assert.NotNull(responseUsers);
        Assert.NotEmpty(responseUsers);
        Assert.Equal(expectedList.Count, responseUsers.Count);
        Assert.All(responseUsers,
            u => {
                Assert.Contains(u, expectedList);
                Assert.Contains(expectedJobTitleContains, u.JobTitle);
            }
        );
    }

    [Fact]
    public async Task GetFiltered_NullFilter_ReturnsFiltered()
    {
        // Arrange
        var user = UserGenerators.Base
            .RuleFor(u => u.FirstName, _ => "Testinio Userini")
            .RuleFor(u => u.Account, _ => null)
            .Generate();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var url = "users/filtered?f.account.eq=null";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseUsers = await response.Content.ReadFromJsonAsync<List<User>>();

        Assert.NotNull(responseUsers);
        Assert.Single(responseUsers);
        Assert.Equal(user, responseUsers.First());

    }

    #endregion GetFiltered

    #region GetFilteredManual

    [Fact]
    public async Task GetFilteredManual_ValidQuery_ReturnsFiltered()
    {
        // Arrange
        var expectedEmailContains = "Yvette";
        var expectedList = await _context.Users
            .Include(u => u.Account)
            .Where(u => u.Email.Contains(expectedEmailContains))
            .ToListAsync();

        var url = "users/filtered/manual";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseUsers = await response.Content.ReadFromJsonAsync<List<User>>();

        Assert.NotNull(responseUsers);
        Assert.NotEmpty(responseUsers);
        Assert.Equal(expectedList.Count, responseUsers.Count);
        Assert.All(responseUsers,
            u => {
                Assert.Contains(u, expectedList);
                Assert.Contains(expectedEmailContains, u.Email);
            }
        );
    }

    #endregion GetFilteredManual

    #region GetPaged

    [Fact]
    public async Task GetPaged_ValidQuery_ReturnsPage()
    {
        // Arrange
        var expectedPageNumber = 3;
        var expectedPageSize = 15;

        var users = await _context.Users
            .Include(u => u.Account)
            .ToListAsync();

        var expectedList = users
            .Skip(expectedPageSize * (expectedPageNumber - 1))
            .Take(expectedPageSize)
            .ToList();

        var expectedTotalPages = Math.Ceiling((double)users.Count / expectedPageSize);

        var url = $"users/paged" +
                    $"?p.number={expectedPageNumber}" +
                    $"&p.size={expectedPageSize}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePage = await response.Content.ReadFromJsonAsync<Page<User>>();

        Assert.NotNull(responsePage);
        Assert.NotEmpty(responsePage.Items);
        Assert.Equal(expectedList.Count, responsePage.Items.Count());
        Assert.Equal(expectedPageNumber, responsePage.CurrentPage);
        Assert.Equal(expectedTotalPages, responsePage.TotalPages);
        Assert.Equal(expectedPageSize, responsePage.PageSize);
        Assert.Equal(users.Count, responsePage.TotalItems);
        Assert.All(responsePage.Items,
            (u, index) => {
                Assert.StrictEqual(u, expectedList[index]);
            }
        );
    }

    #endregion GetPaged

    #region GetComplete

    [Fact]
    public async Task GetComplete_ValidQuery_ReturnsSortedAndFilteredPage()
    {
        // Arrange
        var expectedPageNumber = 1;
        var expectedPageSize = 15;
        var expectedJobTitleContains = "Central";

        var filteredSortedList = await _context.Users
            .Include(u => u.Account)
            .Where(u => u.JobTitle.Contains(expectedJobTitleContains))
            .OrderByDescending(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        var expectedList = filteredSortedList
            .Take(expectedPageSize)
            .ToList();

        var expectedTotalPages = Math.Ceiling((double)filteredSortedList.Count / expectedPageSize);

        var url = $"users/complete" +
                    $"?p.number={expectedPageNumber}" +
                    $"&p.size={expectedPageSize}" +
                    $"&s.firstName=desc" +
                    $"&s.lastName=asc" +
                    $"&f.jobTitle.like={expectedJobTitleContains}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePage = await response.Content.ReadFromJsonAsync<Page<User>>();

        Assert.NotNull(responsePage);
        Assert.NotEmpty(responsePage.Items);
        Assert.Equal(expectedList.Count, responsePage.Items.Count());
        Assert.Equal(expectedPageNumber, responsePage.CurrentPage);
        Assert.Equal(expectedTotalPages, responsePage.TotalPages);
        Assert.Equal(expectedPageSize, responsePage.PageSize);
        Assert.Equal(filteredSortedList.Count, responsePage.TotalItems);
        Assert.All(responsePage.Items,
            (u, index) => {
                Assert.StrictEqual(u, expectedList[index]);
                Assert.Contains(expectedJobTitleContains, u.JobTitle);
            }
        );
    }

    [Fact]
    public async Task GetComplete_NullFilter_ReturnsSortedPage()
    {
        // Arrange
        var expectedPageNumber = 1;
        var expectedPageSize = 15;

        var filteredSortedList = await _context.Users
            .Include(u => u.Account)
            .OrderByDescending(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        var expectedList = filteredSortedList
            .Take(expectedPageSize)
            .ToList();

        var expectedTotalPages = Math.Ceiling((double)filteredSortedList.Count / expectedPageSize);

        var url = $"users/complete" +
                    $"?p.number={expectedPageNumber}" +
                    $"&p.size={expectedPageSize}" +
                    $"&s.firstName=desc" +
                    $"&s.lastName=asc";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePage = await response.Content.ReadFromJsonAsync<Page<User>>();

        Assert.NotNull(responsePage);
        Assert.NotEmpty(responsePage.Items);
        Assert.Equal(expectedList.Count, responsePage.Items.Count());
        Assert.Equal(expectedPageNumber, responsePage.CurrentPage);
        Assert.Equal(expectedTotalPages, responsePage.TotalPages);
        Assert.Equal(expectedPageSize, responsePage.PageSize);
        Assert.Equal(filteredSortedList.Count, responsePage.TotalItems);
        Assert.All(responsePage.Items,
            u => Assert.Contains(u, expectedList)
        );
    }

    [Fact]
    public async Task GetComplete_NullSort_ReturnsFilteredPage()
    {
        // Arrange
        var expectedPageNumber = 1;
        var expectedPageSize = 15;
        var expectedJobTitleContains = "Central";

        var filteredSortedList = await _context.Users
            .Include(u => u.Account)
            .Where(u => u.JobTitle.Contains(expectedJobTitleContains))
            .ToListAsync();

        var expectedList = filteredSortedList
            .Take(expectedPageSize)
            .ToList();

        var expectedTotalPages = Math.Ceiling((double)filteredSortedList.Count / expectedPageSize);

        var url = $"users/complete" +
                    $"?p.number={expectedPageNumber}" +
                    $"&p.size={expectedPageSize}" +
                    $"&f.jobTitle.like={expectedJobTitleContains}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePage = await response.Content.ReadFromJsonAsync<Page<User>>();

        Assert.NotNull(responsePage);
        Assert.NotEmpty(responsePage.Items);
        Assert.Equal(expectedList.Count, responsePage.Items.Count());
        Assert.Equal(expectedPageNumber, responsePage.CurrentPage);
        Assert.Equal(expectedTotalPages, responsePage.TotalPages);
        Assert.Equal(expectedPageSize, responsePage.PageSize);
        Assert.Equal(filteredSortedList.Count, responsePage.TotalItems);
        Assert.All(responsePage.Items,
            u => {
                Assert.Contains(u, expectedList);
                Assert.Contains(expectedJobTitleContains, u.JobTitle);
            }
        );
    }

    [Fact]
    public async Task GetComplete_NoPage_ReturnsSortedAndFilteredWithDefaultPageParameters()
    {
        // Arrange
        var expectedPageNumber = 1;
        var expectedPageSize = 25;
        var expectedJobTitleContains = "Central";

        var filteredSortedList = await _context.Users
            .Include(u => u.Account)
            .Where(u => u.JobTitle.Contains(expectedJobTitleContains))
            .OrderByDescending(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        var expectedList = filteredSortedList
            .Take(expectedPageSize)
            .ToList();

        var expectedTotalPages = Math.Ceiling((double)filteredSortedList.Count / expectedPageSize);

        var url = $"users/complete" +
                    $"?s.firstName=desc" +
                    $"&s.lastName=asc" +
                    $"&f.jobTitle.like={expectedJobTitleContains}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePage = await response.Content.ReadFromJsonAsync<Page<User>>();

        Assert.NotNull(responsePage);
        Assert.NotEmpty(responsePage.Items);
        Assert.Equal(expectedList.Count, responsePage.Items.Count());
        Assert.Equal(expectedPageNumber, responsePage.CurrentPage);
        Assert.Equal(expectedTotalPages, responsePage.TotalPages);
        Assert.Equal(expectedPageSize, responsePage.PageSize);
        Assert.Equal(filteredSortedList.Count, responsePage.TotalItems);
        Assert.All(responsePage.Items,
            (u, index) => {
                Assert.StrictEqual(u, expectedList[index]);
                Assert.Contains(expectedJobTitleContains, u.JobTitle);
            }
        );
    }

    #endregion GetComplete

    #region GetCompleteDto

    [Fact]
    public async Task GetCompleteDto_ValidQuery_ReturnsSortedAndFilteredDtoPage()
    {
        // Arrange
        var expectedPageNumber = 1;
        var expectedPageSize = 15;
        var expectedJobTitleContains = "Central";

        var filteredSortedList = await _context.Users
            .Include(u => u.Account)
            .Where(u => u.JobTitle.Contains(expectedJobTitleContains))
            .OrderByDescending(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => UserDTO.FromUser(u))
            .ToListAsync();

        var expectedList = filteredSortedList
            .Take(expectedPageSize)
            .ToList();

        var expectedTotalPages = Math.Ceiling((double)filteredSortedList.Count / expectedPageSize);

        var url = $"users/complete/dto" +
                    $"?p.number={expectedPageNumber}" +
                    $"&p.size={expectedPageSize}" +
                    $"&s.firstName=desc" +
                    $"&s.lastName=asc" +
                    $"&f.jobTitle.like={expectedJobTitleContains}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePage = await response.Content.ReadFromJsonAsync<Page<UserDTO>>();

        Assert.NotNull(responsePage);
        Assert.NotEmpty(responsePage.Items);
        Assert.Equal(expectedList.Count, responsePage.Items.Count());
        Assert.Equal(expectedPageNumber, responsePage.CurrentPage);
        Assert.Equal(expectedTotalPages, responsePage.TotalPages);
        Assert.Equal(expectedPageSize, responsePage.PageSize);
        Assert.Equal(filteredSortedList.Count, responsePage.TotalItems);
        Assert.All(responsePage.Items,
            (u, index) => {
                Assert.StrictEqual(u, expectedList[index]);
                Assert.Contains(expectedJobTitleContains, u.JobTitle);
            }
        );
    }

    [Fact]
    public async Task GetCompleteDto_NullFilter_ReturnsSortedPage()
    {
        // Arrange
        var expectedPageNumber = 1;
        var expectedPageSize = 15;

        var filteredSortedList = await _context.Users
            .Include(u => u.Account)
            .OrderByDescending(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => UserDTO.FromUser(u))
            .ToListAsync();

        var expectedList = filteredSortedList
            .Take(expectedPageSize)
            .ToList();

        var expectedTotalPages = Math.Ceiling((double)filteredSortedList.Count / expectedPageSize);

        var url = $"users/complete/dto" +
                    $"?p.number={expectedPageNumber}" +
                    $"&p.size={expectedPageSize}" +
                    $"&s.firstName=desc" +
                    $"&s.lastName=asc";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePage = await response.Content.ReadFromJsonAsync<Page<UserDTO>>();

        Assert.NotNull(responsePage);
        Assert.NotEmpty(responsePage.Items);
        Assert.Equal(expectedList.Count, responsePage.Items.Count());
        Assert.Equal(expectedPageNumber, responsePage.CurrentPage);
        Assert.Equal(expectedTotalPages, responsePage.TotalPages);
        Assert.Equal(expectedPageSize, responsePage.PageSize);
        Assert.Equal(filteredSortedList.Count, responsePage.TotalItems);
        Assert.All(responsePage.Items,
            u => Assert.Contains(u, expectedList)
        );
    }

    [Fact]
    public async Task GetCompleteDto_NullSort_ReturnsFilteredPage()
    {
        // Arrange
        var expectedPageNumber = 1;
        var expectedPageSize = 15;
        var expectedJobTitleContains = "Central";

        var filteredSortedList = await _context.Users
            .Include(u => u.Account)
            .Where(u => u.JobTitle.Contains(expectedJobTitleContains))
            .Select(u => UserDTO.FromUser(u))
            .ToListAsync();

        var expectedList = filteredSortedList
            .Take(expectedPageSize)
            .ToList();

        var expectedTotalPages = Math.Ceiling((double)filteredSortedList.Count / expectedPageSize);

        var url = $"users/complete/dto" +
                    $"?p.number={expectedPageNumber}" +
                    $"&p.size={expectedPageSize}" +
                    $"&f.jobTitle.like={expectedJobTitleContains}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePage = await response.Content.ReadFromJsonAsync<Page<UserDTO>>();

        Assert.NotNull(responsePage);
        Assert.NotEmpty(responsePage.Items);
        Assert.Equal(expectedList.Count, responsePage.Items.Count());
        Assert.Equal(expectedPageNumber, responsePage.CurrentPage);
        Assert.Equal(expectedTotalPages, responsePage.TotalPages);
        Assert.Equal(expectedPageSize, responsePage.PageSize);
        Assert.Equal(filteredSortedList.Count, responsePage.TotalItems);
        Assert.All(responsePage.Items,
            u => {
                Assert.Contains(u, expectedList);
                Assert.Contains(expectedJobTitleContains, u.JobTitle);
            }
        );
    }

    [Fact]
    public async Task GetCompleteDto_NoPage_ReturnsSortedAndFilteredWithDefaultPageParameters()
    {
        // Arrange
        var expectedPageNumber = 1;
        var expectedPageSize = 25;
        var expectedJobTitleContains = "Central";

        var filteredSortedList = await _context.Users
            .Include(u => u.Account)
            .Where(u => u.JobTitle.Contains(expectedJobTitleContains))
            .OrderByDescending(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => UserDTO.FromUser(u))
            .ToListAsync();

        var expectedList = filteredSortedList
            .Take(expectedPageSize)
            .ToList();

        var expectedTotalPages = Math.Ceiling((double)filteredSortedList.Count / expectedPageSize);

        var url = $"users/complete/dto" +
                    $"?s.firstName=desc" +
                    $"&s.lastName=asc" +
                    $"&f.jobTitle.like={expectedJobTitleContains}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePage = await response.Content.ReadFromJsonAsync<Page<UserDTO>>();

        Assert.NotNull(responsePage);
        Assert.NotEmpty(responsePage.Items);
        Assert.Equal(expectedList.Count, responsePage.Items.Count());
        Assert.Equal(expectedPageNumber, responsePage.CurrentPage);
        Assert.Equal(expectedTotalPages, responsePage.TotalPages);
        Assert.Equal(expectedPageSize, responsePage.PageSize);
        Assert.Equal(filteredSortedList.Count, responsePage.TotalItems);
        Assert.All(responsePage.Items,
            (u, index) => {
                Assert.Equal(u, expectedList[index]);
                Assert.Contains(expectedJobTitleContains, u.JobTitle);
            }
        );
    }

    #endregion GetCompleteDto

}