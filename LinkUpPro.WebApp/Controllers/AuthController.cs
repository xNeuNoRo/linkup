using LinkUpPro.Application.DTOs.User.Requests;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.ViewModels.AuthViewModels;
using LinkUpPro.WebApp.Filters;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

/// <summary>
/// Controlador de autenticación: Login, Register, ForgotPassword, ResetPassword,
/// ResendActivation, ActivateAccount y Logout.
/// </summary>
[AllowAnonymous]
public class AuthController : BaseController
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAccountService accountService,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger
    )
        : base(currentUserService)
    {
        _accountService = accountService;
        _logger = logger;
    }

    // ====================== LOGIN ======================

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_currentUserService.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (_currentUserService.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var request = new LoginRequest(model.UserName, model.Password);
            await _accountService.LoginAsync(request, model.RememberMe);

            // Mensaje genérico de éxito
            ShowAlert("Bienvenido a LinkUp Pro.");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            // El middleware o el servicio ya formatearon el mensaje al usuario
            // Aquí solo lo mostramos en el ModelState para que se renderice en el form
            _logger.LogWarning(ex, "Login fallido para {UserName}", model.UserName);
            ModelState.AddModelError(string.Empty, GetUserMessage(ex));
            return View(model);
        }
    }

    // ====================== REGISTER ======================

    [HttpGet]
    public IActionResult Register()
    {
        if (_currentUserService.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (_currentUserService.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var request = model.Adapt<RegisterRequest>();
            await _accountService.RegisterAsync(request, GetOrigin());

            // Pantalla informativa de éxito
            TempData["RegisteredUserName"] = model.UserName;
            TempData["RegisteredEmail"] = model.Email;
            return RedirectToAction(nameof(RegistrationConfirmation));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Registro fallido para {UserName}", model.UserName);
            ModelState.AddModelError(string.Empty, GetUserMessage(ex));
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult RegistrationConfirmation()
    {
        ViewData["UserName"] = TempData["RegisteredUserName"];
        ViewData["Email"] = TempData["RegisteredEmail"];
        return View();
    }

    // ====================== FORGOT PASSWORD ======================

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var request = new ForgotPasswordRequest(model.UserName, GetOrigin());
            await _accountService.ForgotPasswordAsync(request);

            // Mensaje genérico (no revela existencia de cuenta)
            ShowAlert(
                "Si el nombre de usuario corresponde a una cuenta registrada, recibirá un enlace para restablecer su contraseña."
            );

            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Forgot password fallido para {UserName}", model.UserName);
            ModelState.AddModelError(string.Empty, GetUserMessage(ex));
            return View(model);
        }
    }

    // ====================== RESET PASSWORD ======================

    [HttpGet]
    public IActionResult ResetPassword(string? userId = null, string? token = null)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            // Si no hay token/userId, redirigir a ForgotPassword
            ShowWarning("El enlace para restablecer la contraseña no es válido.");
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(
            new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            }
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var request = new ResetPasswordRequest(
                model.UserId,
                model.Token,
                model.Password,
                model.ConfirmPassword
            );
            await _accountService.ResetPasswordAsync(request);

            ShowAlert("Su contraseña fue restablecida correctamente. Ya puede iniciar sesión.");
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reset password fallido");
            ModelState.AddModelError(string.Empty, GetUserMessage(ex));
            return View(model);
        }
    }

    // ====================== RESEND ACTIVATION ======================

    [HttpGet]
    public IActionResult ResendActivation()
    {
        return View(new ResendActivationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendActivation(ResendActivationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var request = new ResendActivationRequest(model.UserName, GetOrigin());
            await _accountService.ResendActivationAsync(request);

            // Mensaje genérico (no revela existencia/estado)
            ShowAlert(
                "Si la cuenta existe y todavía no ha sido activada, recibirá un nuevo enlace de activación."
            );

            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Resend activation fallido para {UserName}", model.UserName);
            ModelState.AddModelError(string.Empty, GetUserMessage(ex));
            return View(model);
        }
    }

    // ====================== ACTIVATE ACCOUNT ======================

    [HttpGet]
    public async Task<IActionResult> ActivateAccount(string userId, string token)
    {
        var model = new ActivateAccountViewModel
        {
            UserId = userId,
            Token = token
        };

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            model.IsSuccess = false;
            model.Message = "El enlace de activación no es válido.";
            return View(model);
        }

        try
        {
            await _accountService.ConfirmAccountAsync(userId, token);
            model.IsSuccess = true;
            model.Message = "Su cuenta fue activada correctamente. Ya puede iniciar sesión.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Activación fallida");
            model.IsSuccess = false;
            model.Message = GetUserMessage(ex);
        }

        return View(model);
    }

    // ====================== LOGOUT ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize]
    public async Task<IActionResult> Logout()
    {
        await _accountService.SignOutAsync();
        ShowInfo("Ha cerrado sesión correctamente.");
        return RedirectToAction(nameof(Login));
    }

    // ====================== HELPERS ======================

    private string GetOrigin()
    {
        return $"{Request.Scheme}://{Request.Host.Value}";
    }

    /// <summary>
    /// Extrae el mensaje amigable para el usuario desde una excepción.
    /// Las DomainException tienen un mensaje diseñado para mostrarse al usuario.
    /// DomainValidationException expone múltiples errores específicos: se concatenan.
    /// </summary>
    private static string GetUserMessage(Exception ex)
    {
        if (ex is LinkUpPro.Domain.Exceptions.DomainValidationException validationEx
            && validationEx.ValidationErrors.Count > 0)
        {
            return string.Join(" ", validationEx.ValidationErrors.Select(e => e.Message));
        }

        return ex switch
        {
            LinkUpPro.Domain.Exceptions.DomainException dex => dex.Message,
            _ => "Ocurrió un error al procesar la solicitud. Inténtelo nuevamente."
        };
    }
}
