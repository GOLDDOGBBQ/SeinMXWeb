using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;




namespace SEINMX.Clases;

public class ApplicationController  : Controller
{
    private string  ApiName ;
    private int IdUsuarioPrimaryKey ;
    private string UserId ;
    private bool Admin ;


    public string GetApiName()
    {
        return ApiName;
    }
    public string GetUserId()
    {
        return UserId;
    }
    public int GetIdUsuarioPrimaryKey()
    {
        return IdUsuarioPrimaryKey;
    }


    public bool GetIsAdmin()
    {
        return Admin;
    }


    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? sAdmion = User.FindFirstValue("Admin");
        string? IdUsuario = User.FindFirstValue("IdUsuario");

        var serviceProvider = context.HttpContext.RequestServices;

        var actionContextAccessor = serviceProvider.GetRequiredService<IActionContextAccessor>();
        var rd = actionContextAccessor.ActionContext.RouteData;
        var controller = rd.Values["controller"];
        var actionName = rd.Values["action"]!.ToString()!.ToUpper();

        ApiName =    controller + "." + actionName;
        UserId = userId;

        if (!bool.TryParse(sAdmion, out Admin))
        {
            Admin = false;
        }
        if (!int.TryParse(IdUsuario, out IdUsuarioPrimaryKey))
        {
            IdUsuarioPrimaryKey = 0;
        }

    }



    
}

