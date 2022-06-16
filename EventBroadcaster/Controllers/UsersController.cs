using Commun;
using Commun.Models;
using EventBroadcaster.Entities;
using EventBroadcaster.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EventBroadcaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(InternalTokenAuth))]
    public class UsersController : ControllerBase
    {
        private readonly DataDbContext _context;

        public UsersController(DataDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<User>>> Get([FromQuery] int page = 1, [FromQuery] int size = 1000)
        {
            if (page < 1 || size < 1) return NotFound();
            var total = await _context.User.AsNoTracking().CountAsync();

            var skip = new SqlParameter("@skip", (page - 1) * size);
            var take = new SqlParameter("@take", size);
            string sql = $"SELECT * FROM [DataDb].[dbo].[User] Order by [Id]  OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
            var users = await _context.User.FromSqlRaw(sql, skip, take).ToListAsync();
            var totalPages = total / size;
            if (total % size > 0) totalPages++;

            return new PagedResult<User>
            {
                Total = total,
                Page = page,
                TotalPages = totalPages,
                Data = users
            };
        }
    }
}
