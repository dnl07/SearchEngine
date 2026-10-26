using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.Api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Returns 200 while the service is running.
        /// </summary>
        [HttpGet]
        public IActionResult GetHealth() => Ok();
    }
}
