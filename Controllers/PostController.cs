using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class PostController:Controller
    {
        private readonly ILogger<GetController> _logger;

        public PostController(ILogger<GetController> logger)
        {
            _logger = logger;
        }
        
        public class Result
        {
            public string Description { get; set; }
        }
        
        [HttpPost]
        public JsonResult Post(string text)
        {
            var result = new Result()
            {
                Description = text
            };
            return Json(result);
        }
    }
}