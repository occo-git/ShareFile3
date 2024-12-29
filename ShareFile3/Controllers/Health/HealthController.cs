using Microsoft.AspNetCore.Mvc;

namespace ShareFile.Controllers.Health
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Healthy");
        }
    }
}