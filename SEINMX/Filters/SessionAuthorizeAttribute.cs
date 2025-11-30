using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SEINMX.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuario = context.HttpContext.Session.GetInt32("IdUsuario");
            if (usuario == null)
            {
                context.Result = new RedirectToActionResult("AccesoDenegado", "Cuenta", null);
            }
            base.OnActionExecuting(context);
        }
    }
}