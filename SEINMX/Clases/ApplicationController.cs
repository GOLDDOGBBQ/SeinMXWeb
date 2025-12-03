using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;



namespace SEINMX.Clases;

public class ApplicationController  : Controller
{
    private string  ApiName ;
    private string UserId ;


    public string GetApiName()
    {
        return ApiName;
    }
    public string GetUserId()
    {
        return UserId;
    }


    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var serviceProvider = context.HttpContext.RequestServices;

        var actionContextAccessor = serviceProvider.GetRequiredService<IActionContextAccessor>();
        var rd = actionContextAccessor.ActionContext.RouteData;
        var controller = rd.Values["controller"];
        var actionName = rd.Values["action"]!.ToString()!.ToUpper();

        ApiName =    controller + "." + actionName;
        UserId = userId;

    }



    
}

