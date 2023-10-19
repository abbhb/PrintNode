using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PrintNode.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public string Get() => "ok";
    }
}
