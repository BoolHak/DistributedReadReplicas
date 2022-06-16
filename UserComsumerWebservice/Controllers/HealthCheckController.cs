using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserComsumerWebservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        public IActionResult Index()
        {
            if (Config.State != "OK")
                return NotFound(Config.State);

            return Ok(Config.State);
        }
    }
}
