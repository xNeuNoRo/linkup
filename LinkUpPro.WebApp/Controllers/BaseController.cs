using LinkUpPro.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

/// <summary>
/// Controlador base del cual deben heredar todos los controladores que requieran autenticación.
/// Centraliza utilidades repetitivas como inyección de CurrentUser y alertas SweetAlert.
/// </summary>
public abstract class BaseController : Controller
{
    protected readonly ICurrentUserService _currentUserService;

    protected BaseController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Muestra una notificación SweetAlert en la siguiente vista cargada (Toast en top-end).
    /// </summary>
    /// <param name="message">El mensaje a mostrar</param>
    /// <param name="type">El tipo de alerta (success, error, warning, info)</param>
    protected void ShowAlert(string message, string type = "success")
    {
        TempData["SweetAlertType"] = type;
        TempData["SweetAlertMessage"] = message;
    }

    protected void ShowError(string message) => ShowAlert(message, "error");
    protected void ShowWarning(string message) => ShowAlert(message, "warning");
    protected void ShowInfo(string message) => ShowAlert(message, "info");
}
