using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.WebApp.Filters;

/// <summary>
/// Filtro de autorización para validar:
/// 1. Que exista una sesión activa (ASP.NET Core Identity cookie)
/// 2. Que la cuenta esté activa (IsActive = true)
/// 3. Que no haya pasado el tiempo de inactividad (30 min default, 7 días si RememberMe)
///
/// Redirige al Login con mensaje si alguna validación falla.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var currentUserService =
            httpContext.RequestServices.GetService(typeof(ICurrentUserService))
            as ICurrentUserService;

        if (currentUserService == null || !currentUserService.IsAuthenticated)
        {
            // No autenticado → redirigir al Login
            context.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Auth", action = "Login" })
            );
            return;
        }

        // Validar que la cuenta esté activa
        var isActive = await currentUserService.IsActiveAsync(httpContext.RequestAborted);
        if (!isActive)
        {
            SetErrorAndRedirect(
                context,
                "Su cuenta se encuentra inactiva. Debe activarla mediante el enlace enviado a su correo electrónico.",
                "Account.Inactive"
            );
            return;
        }
    }

    private static void SetErrorAndRedirect(
        AuthorizationFilterContext context,
        string message,
        string code = "Session.Expired"
    )
    {
        var tempDataFactory =
            context.HttpContext.RequestServices.GetService<ITempDataDictionaryFactory>();
        if (tempDataFactory != null)
        {
            var tempData = tempDataFactory.GetTempData(context.HttpContext);
            tempData["ErrorMessage"] = message;
            tempData["ErrorCode"] = code;
            tempData.Save();
        }

        context.Result = new RedirectToRouteResult(
            new RouteValueDictionary(new { controller = "Auth", action = "Login" })
        );
    }
}
