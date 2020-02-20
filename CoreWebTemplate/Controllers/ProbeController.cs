using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreWebTemplate.Controllers {
    [Route("api/probe")]
    public class ProbeController : ControllerBase {
        public ProbeController(ILogger<ProbeController> logger) : base(logger) {
        }

        [HttpGet("server-version")]
        public IActionResult ServerVersion() {
            return Json(Program.ServerVersion);
        }
    }
}
