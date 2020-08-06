using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("HelloWorld")]
    [Produces("application/json")]
    public class HelloWorldController : Controller
    {
        private readonly ILogger<HelloWorldController> _logger;

        public HelloWorldController(ILogger<HelloWorldController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public JsonResult Get()
        {
            return Json(new { text = "Project Mesh Web API" });
        }

        [HttpPost]
        public JsonResult Post(string text)
        {
            return Json(new { text = text });
        }
    }
}