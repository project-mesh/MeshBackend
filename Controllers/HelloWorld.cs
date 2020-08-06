using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("HelloWorld")]
    [Produces("application/json")]
    public class HelloWorld:Controller
    {
        private readonly ILogger<HelloWorld> _logger;

        public HelloWorld(ILogger<HelloWorld> logger)
        {
            _logger = logger;
        }

        public class Result
        {
            public string Description { get; set; }
        }
        
        [HttpPost]
        public JsonResult Post1599655068(string text)
        {
            var result = new Result()
            {
                Description = text
            };
            return Json(result);
        }
        
        [HttpGet]
        public JsonResult Get()
        {
            var result = new Result()
            {
                Description = "Project Mesh Web API"
            };
            return Json(result);
        }
    }
}