using Microsoft.AspNetCore.Mvc;

namespace EventBroadcaster.Controllers
{
    public class HealthCheck : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
