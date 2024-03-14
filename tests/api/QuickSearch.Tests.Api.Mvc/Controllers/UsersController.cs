using QuickSearch.Tests.Api.Common.Data;
using QuickSearch.Tests.Api.Common.Data.Models;
using QuickSearch.Tests.Api.Common.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickSearch.Filter;
using QuickSearch.Sort;
using QuickSearch.Pagination;

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
                .Sort(request)
                .ToListAsync();

        [HttpGet("filtered")]
        public Task<List<User>> GetFiltered([FromQuery(Name = "f")] FilterOptions<User> request)
            => _context.Users
                .Include(u => u.Account)
                .Filter(request)
                .ToListAsync();

        [HttpGet("filtered/manual")]
        public Task<List<User>> GetFilteredManual()
        {
            var filter = new FilterOptions<User>();
            filter.AddFilter(u => u.Email, FilterType.Like, "Yvette");

            return _context.Users
                .Include(u => u.Account)
                .Filter(filter)
                .ToListAsync();
        }

        [HttpGet("paged")]
        public Task<Page<User>> GetPaged([FromQuery(Name = "p")] PageOptions request)
            => _context.Users
                .Include(u => u.Account)
                .ToPageAsync(request);

        [HttpGet("complete")]
        public Task<Page<User>> GetComplete([FromQuery] SearchRequest<User> request)
            => _context.Users
                .Include(u => u.Account)
                .Filter(request.Filter)
                .Sort(request.Sort)
                .ToPageAsync(request.Page);
        
        [HttpGet("complete/dto")]
        public async Task<Page<UserDTO>> GetCompleteDto(
            [FromQuery] SearchRequest<UserDTO> request
        )
        {
            var result = await _context.Users
                .Include(u => u.Account)
                .Filter(request.Filter?.MapTo<User>())
                .Sort(request.Sort?.MapTo<User>())
                .ToPageAsync(request.Page);

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