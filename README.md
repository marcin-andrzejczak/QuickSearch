<a name="readme-top"></a>


<!-- PROJECT SHIELDS -->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">

  <h3 align="center">QuickSearch</h3>

  <p align="center">
    .NET 6 library to make sorting, filtering and paging easier
    <br />
    <a href="https://github.com/marcin-andrzejczak/QuickSearch"><strong>Explore the docs »</strong></a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details open>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#installation">Installation</a></li>
    <li>
      <a href="#usage">Usage</a>
      <ul>
        <li><a href="#example-request">Example request</a></li>
        <li><a href="#response-format">Response format</a></li>
      </ul>
    </li>
    <li><a href="#mapping">Mapping</a></li>
    <li>
      <a href="#validation">Validation</a>
      <ul>
        <li><a href="#example-validation-error">Example validation error</a></li>
      </ul>
    </li>
    <li>
      <a href="#mapping-to-query-string">Mapping to query string</a>
      <ul>
        <li>
            <a href="#single-option-query-string">Single option query string</a>
            <ul>
                <li><a href="#page">Page</a></li>
                <li><a href="#page">Filter</a></li>
                <li><a href="#page">Sort</a></li>
            </ul>
        </li>
        <li><a href="#multiple-options-at-once">Multiple options at once</a></li>
      </ul>
    </li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

Since many REST APIs need the functionality to sort, filter and page data, there should be a simple way to do such a trivial task without lots of hardcoded filters and conditions. **QuickSearch** provides exactly this - simple way to fetch data from your API with a simple GET request.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- INSTALLATION -->
## Installation

TBD when it'll be available on NuGet

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE -->
## Usage

To start supporting sorted/filtered/paginated queries, all you need to do is create an endpoint that will receive `SortOptions<TEntity>`, `FilterOptions<TEntity>` or `PageOptions` as a query parameter. These options can be then given directly to LINQ extensions without any additional mapping whatsoever:

```csharp
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
        => _context = context;

    [HttpGet("sorted")]
    public Task<List<User>> GetSorted([FromQuery(Name = "s")] SortOptions<User> request)
        => _context.Users.Sort(request).ToListAsync();

    [HttpGet("filtered")]
    public Task<List<User>> GetFiltered([FromQuery(Name = "f")] FilterOptions<User> request)
        => _context.Users.Filter(request).ToListAsync();

    [HttpGet("paged")]
    public Task<Page<User>> GetPaged([FromQuery(Name = "p")] PageOptions request)
        => _context.Users.ToPageAsync(request);
}
```

To have all options in one request, you can create a request class that will contain all of those and set it as endpoint parameter:

```csharp
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public Task<Page<User>> GetUsers([FromQuery] SearchRequest request)
    {
        var query = _context.Users.Filter(request.Filter);

        if (request.Sort is not null)
            query = query.Sort(request.Sort);

        return query.ToPageAsync(request.Page);
    }
}

public class SearchRequest
{
    [FromQuery(Name = "p")]
    public PageOptions? Page { get; set; }
    
    [FromQuery(Name = "s")]
    public SortOptions<User>? Sort { get; set; }

    [FromQuery(Name = "f")]
    public FilterOptions<User>? Filter { get; set; }
}
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- EXAMPLE REQUEST -->
### Example request

Example request for such endpoint will look like following:
```bash
curl --location "https://localhost:7032/users/paged\
    ?p.number=1\
    &p.size=15\
    &s.firstName=desc\
    &s.lastName=asc\
    &f.email.like=Adrian"
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- RESPONSE FORMAT -->
### Response format

Paged response will come in a following format:

```json
{
    "items": [
        ...
    ],
    "currentPage": <int>,
    "pageSize": <int>,
    "totalItems": <int>,
    "totalPages": <int>
}
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MAPPING -->
## Mapping

In case if you don't necessarily want to expose your data model in the API, you can create a map between types to separate search model from data model. Simply create a class extending `AbstractPropertyMap<TIn, TOut>`, and declare mappings in the constructor.

```csharp
public class Account
{
    public int Balance { get; set; }
}

public class User
{
    public string FirstName { get; set; }
    public Account Account { get; set; }
}

public class UserSearch
{
    public string Name { get; set; }
    public int AccountBalance { get; set; }
}

public class UserPaginationMap : AbstractPropertyMap<UserSearch, User>
{
    public UserPaginationMap()
    {
        Map(dto => dto.Name, u => u.FirstName);
        Map(dto => dto.AccountBalance, u => u.Account!.Balance);
    }
}
```

For result mapping, `Page` class currently supports simple `MapTo` method with a delegate parameter, which will return a page of the desired type.

Example of options and page mapping:

```csharp
[HttpGet("users")]
public async Page<UserDTO> GetUsers(
    [FromQuery] SortOptions<UserSearch> sortOptions,
    [FromQuery] FilterOptions<UserSearch> filterOptions
    [FromQuery] PageOptions pageOptions
)
{
    var userSortOptions = sortOptions.MapTo<User>();
    var userFilterOptions = filterOptions.MapTo<User>();

    var result = await _context.Users.Include(u => u.Account)
        .Filter(userFilterOptions)
        .Sort(userSortOptions)
        .ToPageAsync(pageOptions);

    return result.MapTo(u => UserDTO.FromUser(u));
}
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- VALIDATION -->
## Validation

**QuickSearch** requests are validated out of the box on model binding. `SortOptions<TEntity>` and `FilterOptions<TEntity>` are validated also against the entity they are bound to, to check if all the properties on filters and sorters passed to the request are present on the model you want to filter/sort.



<!-- EXAMPLE VALIDATION ERROR -->
### Example validation error

URL:
```
https://localhost:7032/users
    ?p.number=-1
    &p.size=-1
    &s.notexistingproperty=asc
    &s.firstName=notexistingsorter
    &f.notexistingproperty.eq=Test
    &f.firstName.notexistingfilter=Test
```

Response:
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "traceId": "00-c9bf80bdbbcb14a4001e3084f299a882-205455575e3d01ba-00",
    "errors": {
        "p.Size": [
            "The field Size must be between 1 and 2147483647."
        ],
        "p.Number": [
            "The field Number must be between 1 and 2147483647."
        ],
        "s.firstName": [
            "Unrecognized sort direction value"
        ],
        "s.notexistingproperty": [
            "Property does not exist on entity 'User'"
        ],
        "f.firstName.notexistingfilter": [
            "Invalid filter type"
        ],
        "f.notexistingproperty.eq": [
            "Property does not exist on entity 'User'"
        ]
    }
}
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MAPPING TO QUERY STRING -->
## Mapping to query string

If ever needed to pass options to another service request via query parameters, it's possible via separate options or whole using a builder.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- SINGLE OPTION QUERY STRING -->
### Single option query string

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- PAGE -->
#### Page
```csharp
 var pageOptions = new PageOptions
 {
     Number = 10,
     Size = 40,
 };

var queryString = pageOptions.ToQueryString("p");
```

`queryString` will be equal to `p.Number=10&p.Size=40`

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- FILTER -->
#### Filter
```csharp
var sortOptions = new FilterOptions<User>()
    .AddFilter(u => u.FirstName, FilterType.Eq, "John")
    .AddFilter(u => u.LastName, FilterType.Like, "DoePerhaps");

var queryString = filterOptions.ToQueryString("f");
```

`queryString` will be equal to `f.FirstName.Eq=John&f.LastName.Like=DoePerhaps`

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- SORT -->
#### Sort
```csharp
var options = new SortOptions()
    .AddSort(u => u.Id, SortDirection.Desc)
    .AddSort(u => u.Name, SortDirection.Asc);

var queryString = options.ToQueryString("s");
```

`queryString` will be equal to `s.Id=Desc&s.Name=Asc`

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MULTIPLE OPTIONS AT ONCE -->
### Multiple options at once
```csharp
var pageOptions = new PageOptions
{
    Number = 10,
    Size = 40
};

var filterOptions = new FilterOptions<User>()
    .AddFilter(u => u.Id, FilterType.Eq, "someid")
    .AddFilter(u => u.Name, FilterType.Like, "somename");

var sortOptions = new SortOptions<User>()
    .AddSort(u => u.Id, SortDirection.Desc)
    .AddSort(u => u.Name, SortDirection.Asc);

var builder = new PaginationQueryBuilder<User>()
    .Page("p", pageOptions)
    .Filter("f", filterOptions)
    .Sort("s", sortOptions);

var queryString = builder.ToQueryString();
```

`queryString` will be equal to `p.Number=10&p.Size=40&f.Id.Eq=someid&f.Name.Like=somename&s.Id=Desc&s.Name=Asc"`

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ROADMAP -->
## Roadmap

- [x] Add option to map DTOs to actual models to not force API to expose data model
- [x] Add tests
    - [x] Unit tests
    - [x] Integration tests
- [ ] Add option to easily overwrite validation logic
- [ ] Make it work properly with minimal APIs
- [ ] Add better swagger support
    - [ ] Better controls display
    - [ ] Show all accepted fields for given entity

See the [open issues](https://github.com/marcin-andrzejczak/QuickSearch/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- LICENSE -->
## License

Distributed under the Apache-2.0 License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/marcin-andrzejczak/QuickSearch.svg?style=for-the-badge
[contributors-url]: https://github.com/marcin-andrzejczak/QuickSearch/graphs/contributors

[forks-shield]: https://img.shields.io/github/forks/marcin-andrzejczak/QuickSearch.svg?style=for-the-badge
[forks-url]: https://github.com/marcin-andrzejczak/QuickSearch/network/members

[stars-shield]: https://img.shields.io/github/stars/marcin-andrzejczak/QuickSearch.svg?style=for-the-badge
[stars-url]: https://github.com/marcin-andrzejczak/QuickSearch/stargazers

[issues-shield]: https://img.shields.io/github/issues/marcin-andrzejczak/QuickSearch.svg?style=for-the-badge
[issues-url]: https://github.com/marcin-andrzejczak/QuickSearch/issues

[license-shield]: https://img.shields.io/github/license/marcin-andrzejczak/QuickSearch.svg?style=for-the-badge
[license-url]: https://github.com/marcin-andrzejczak/QuickSearch/blob/master/LICENSE.txt
