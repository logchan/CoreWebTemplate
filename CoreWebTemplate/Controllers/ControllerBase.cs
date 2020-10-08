using System.Security.Claims;
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

        internal bool ProcessUser() {
            if (!User.Identity.IsAuthenticated) {
                return false;
            }

            var username = User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            // TODO check if user is allowed
            // TODO set user model, etc

            return true;
        }
    }
}
