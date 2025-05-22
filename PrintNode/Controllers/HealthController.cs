using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PrintNode.Controllers
{
    [Route("api/[controller]")]
    [EnableCors]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public string Get() => "ok";
    }
}
