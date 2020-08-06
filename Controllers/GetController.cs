using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class GetController :Controller
    {
        private readonly ILogger<GetController> _logger;

        public GetController(ILogger<GetController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public JsonResult Get()
        {
            return Json(new {description = "Project Mesh Web API"});
        }
    }
}