using QuickSearch.Tests.Api.Common.Data;
using QuickSearch.Tests.Api.Common.Data.Models;
using QuickSearch.Tests.Api.Common.Dtos;
using QuickSearch.Extensions;
using QuickSearch.Models;
using QuickSearch.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QuickSearch.Tests.Api.Mvc.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
            => _context = context;

        [HttpGet("sorted")]
        public Task<List<User>> GetSorted([FromQuery(Name = "s")] SortOptions<User> request)
            => _context.Users
                .Include(u => u.Account)
                .Sorted(request)
                .ToListAsync();

        [HttpGet("filtered")]
        public Task<List<User>> GetFiltered([FromQuery(Name = "f")] FilterOptions<User> request)
            => _context.Users
                .Include(u => u.Account)
                .Filtered(request)
                .ToListAsync();

        [HttpGet("filtered/manual")]
        public Task<List<User>> GetFilteredManual()
        {
            var filter = new FilterOptions<User>();
            filter.AddFilter(u => u.Email, FilterType.Like, "Yvette");

            return _context.Users
                .Include(u => u.Account)
                .Filtered(filter)
                .ToListAsync();
        }

        [HttpGet("paged")]
        public Task<Page<User>> GetPaged([FromQuery(Name = "p")] PageOptions request)
            => _context.Users
                .Include(u => u.Account)
                .PagedAsync(request);

        [HttpGet("complete")]
        public Task<Page<User>> GetComplete([FromQuery] SearchRequest<User> request)
            => _context.Users
                .Include(u => u.Account)
                .Filtered(request.Filter)
                .Sorted(request.Sort)
                .PagedAsync(request.Page);
        
        [HttpGet("complete/dto")]
        public async Task<Page<UserDTO>> GetCompleteDto(
            [FromQuery] SearchRequest<UserDTO> request
        )
        {
            var result = await _context.Users
                .Include(u => u.Account)
                .Filtered(request.Filter?.MapTo<User>())
                .Sorted(request.Sort?.MapTo<User>())
                .PagedAsync(request.Page);

            return result.MapTo(u => UserDTO.FromUser(u));
        }
    }

    public class SearchRequest<TEntity> where TEntity : class
    {
        [FromQuery(Name = "p")]
        public PageOptions? Page { get; set; }
        
        [FromQuery(Name = "s")]
        public SortOptions<TEntity>? Sort { get; set; }

        [FromQuery(Name = "f")]
        public FilterOptions<TEntity>? Filter { get; set; }
    }
}