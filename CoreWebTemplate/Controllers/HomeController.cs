using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreWebTemplate.Controllers {
    [Route("")]
    public sealed class HomeController : ControllerBase {
        public HomeController(ILogger<HomeController> logger) : base(logger) { }

        [HttpGet]
        public IActionResult Index() {
            return View();
        }
    }
}
