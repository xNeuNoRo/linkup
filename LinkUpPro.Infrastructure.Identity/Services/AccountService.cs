using System.Security.Claims;
using System.Text;
using LinkUpPro.Application.DTOs.User.Requests;
using LinkUpPro.Application.DTOs.User.Responses;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.Models.Emails;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Infrastructure.Identity.Entities;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace LinkUpPro.Infrastructure.Identity.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IFileService _fileService;
    private readonly IProfileService _profileService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IEmailService emailService,
        IFileService fileService,
        IProfileService profileService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AccountService> logger
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _fileService = fileService;
        _profileService = profileService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, bool rememberMe)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user is null)
            throw new DomainValidationException(
                "UserName",
                "El nombre de usuario o la contraseña son incorrectos.",
                "Auth.LoginFailed"
            );

        if (!user.IsActive || !user.EmailConfirmed)
            throw new DomainValidationException(
                "Account",
                "Su cuenta se encuentra inactiva. Debe activarla mediante el enlace enviado a su correo electrónico.",
                "Auth.AccountInactive"
            );

        // Limpiar sesiones anteriores para invalidar cookies previas al cambiar contraseña
        await _userManager.UpdateSecurityStampAsync(user);

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            request.Password,
            rememberMe,
            lockoutOnFailure: true
        );

        if (result.IsLockedOut)
            throw new DomainValidationException(
                "Account",
                "La cuenta se encuentra bloqueada temporalmente debido a varios intentos fallidos. Inténtelo nuevamente en 15 minutos o restablezca su contraseña.",
                "Auth.AccountLocked"
            );

        if (!result.IsLockedOut && !result.Succeeded)
            throw new DomainValidationException(
                "UserName",
                "El nombre de usuario o la contraseña son incorrectos.",
                "Auth.LoginFailed"
            );

        // Refrescar la cookie con claims personalizados para SessionAuthorize
        user.LastActivityAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var customClaims = BuildCustomClaims(user, rememberMe, roles);
        await _signInManager.SignInWithClaimsAsync(user, rememberMe, customClaims);

        return user.Adapt<AuthResponseDto>() with { Roles = [.. roles] };
    }

    private static IEnumerable<Claim> BuildCustomClaims(
        AppUser user,
        bool rememberMe,
        IList<string> roles
    )
    {
        var claims = new List<Claim>
        {
            new("FirstName", user.FirstName ?? string.Empty),
            new("LastName", user.LastName ?? string.Empty),
            new(
                "ProfilePicturePath",
                user.ProfilePicturePath ?? string.Empty
            ),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, string origin)
    {
        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
            throw new DomainValidationException(
                "UserName",
                "Este nombre de usuario ya se encuentra registrado.",
                "Auth.UserNameTaken"
            );

        var existingEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmail is not null)
            throw new DomainValidationException(
                "Email",
                "Este correo electronico ya se encuentra registrado.",
                "Auth.EmailTaken"
            );

        if (!_fileService.IsImageValid(request.ProfilePictureFile))
            throw new DomainValidationException(
                "ProfilePicture",
                "El archivo seleccionado no tiene un formato de imagen valido o supera los 5 MB.",
                "Auth.InvalidImage"
            );

        var profilePicturePath = await _fileService.UploadFileAsync(
            request.ProfilePictureFile,
            "profiles"
        );

        var user = new AppUser
        {
            UserName = request.UserName,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            ProfilePicturePath = profilePicturePath,
            EmailConfirmed = false,
            IsActive = false,
        };

        user.SetEmail(request.Email);
        user.SetPhoneNumber(request.PhoneNumber);

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            throw new DomainValidationException(
                "Registration",
                createResult.Errors.FirstOrDefault()?.Description
                    ?? "Error al registrar el usuario.",
                "Auth.RegistrationFailed"
            );

        await _userManager.AddToRoleAsync(user, "User");

        var verificationUri = await GenerateActivationUri(user, origin);
        var emailSent = await _emailService.SendEmailAsync(
            user.Email!,
            "Activa tu cuenta en LinkUp Pro",
            "AccountActivation",
            new ActivationEmailModel(user.UserName!, verificationUri)
        );

        if (!emailSent)
        {
            _logger.LogWarning(
                "Registro completado para {UserName}, pero no se pudo enviar el correo de activacion a {Email}. El usuario debera usar 'Reenviar correo de activacion'.",
                user.UserName,
                user.Email
            );
        }

        return user.Adapt<AuthResponseDto>();
    }

    public async Task<AuthResponseDto> ConfirmAccountAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new DomainValidationException(
                "Token",
                "El enlace de activacion no es valido o ya fue utilizado.",
                "Auth.InvalidToken"
            );

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            throw new DomainValidationException(
                "Token",
                "El enlace de activacion no es valido o ya fue utilizado.",
                "Auth.InvalidToken"
            );

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        var loginUrl = GetOrigin() + "/Auth/Login";
        await _emailService.SendEmailAsync(
            user.Email!,
            "Bienvenido a LinkUp Pro",
            "Welcome",
            new WelcomeModel(user.UserName!, loginUrl)
        );

        return user.Adapt<AuthResponseDto>();
    }

    public async Task<AuthResponseDto> ResendActivationAsync(ResendActivationRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is null || user.IsActive || user.EmailConfirmed)
            return new AuthResponseDto(
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                false
            );

        if (
            user.LastActivationEmailSentAt.HasValue
            && user.LastActivationEmailSentAt.Value.Add(DomainConstants.ActivationResendCooldown)
                > DateTime.UtcNow
        )
            throw new DomainValidationException(
                "Resend",
                "Debe esperar 5 minutos antes de solicitar un nuevo enlace de activacion.",
                "Auth.ResendCooldown"
            );

        var verificationUri = await GenerateActivationUri(user, request.Origin);
        await _emailService.SendEmailAsync(
            user.Email!,
            "Activa tu cuenta en LinkUp Pro",
            "AccountActivation",
            new ActivationEmailModel(user.UserName!, verificationUri)
        );

        user.LastActivationEmailSentAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return user.Adapt<AuthResponseDto>();
    }

    public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is not null)
        {
            var resetUri = await GeneratePasswordResetUri(user, request.Origin);
            await _emailService.SendEmailAsync(
                user.Email!,
                "Restablece tu contraseña en LinkUp Pro",
                "PasswordReset",
                new ResetPasswordEmailModel(user.UserName!, resetUri)
            );
        }

        return new AuthResponseDto(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            null,
            false
        );
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
            throw new DomainValidationException(
                "Token",
                "El enlace para restablecer la contraseña no es valido o ya fue utilizado.",
                "Auth.InvalidToken"
            );

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.Password);

        if (!result.Succeeded)
            throw new DomainValidationException(
                "Token",
                "El enlace para restablecer la contraseña no es valido o ya fue utilizado.",
                "Auth.InvalidToken"
            );

        await _userManager.UpdateSecurityStampAsync(user);

        return user.Adapt<AuthResponseDto>();
    }

    public Task SignOutAsync()
    {
        return _signInManager.SignOutAsync();
    }

    private async Task<string> GenerateActivationUri(AppUser user, string origin)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var route = $"{origin}/Auth/ActivateAccount";
        var uri = QueryHelpers.AddQueryString(route, "userId", user.Id);
        uri = QueryHelpers.AddQueryString(uri, "token", encodedToken);
        return uri;
    }

    private async Task<string> GeneratePasswordResetUri(AppUser user, string origin)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var route = $"{origin}/Auth/ResetPassword";
        var uri = QueryHelpers.AddQueryString(route, "userId", user.Id);
        uri = QueryHelpers.AddQueryString(uri, "token", encodedToken);
        return uri;
    }

    private string GetOrigin()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return request != null
            ? $"{request.Scheme}://{request.Host.Value}"
            : "https://localhost";
    }
}
