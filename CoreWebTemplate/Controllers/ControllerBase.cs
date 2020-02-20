using CoreWebTemplate.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreWebTemplate.Controllers {
    public abstract class ControllerBase : Controller {
        protected readonly ILogger _logger;
        protected readonly ServerConfig _config = Program.ServerConfig;

        protected ControllerBase(ILogger logger) {
            _logger = logger;
        }
    }
}
