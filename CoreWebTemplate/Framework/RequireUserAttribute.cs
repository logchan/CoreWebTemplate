using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ControllerBase = CoreWebTemplate.Controllers.ControllerBase;

namespace CoreWebTemplate.Framework {
    public sealed class RequireUserAttribute : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext context) {
            if (!(context.Controller is ControllerBase controller)) {
                throw new InvalidOperationException("RequireUser is applied to a controller that does not derive from ControllerBase");
            }

            if (!controller.ProcessUser()) {
                context.Result = new StatusCodeResult(401);
            }
        }
    }
}
