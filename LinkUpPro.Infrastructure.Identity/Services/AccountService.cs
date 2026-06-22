using System.Text;
using LinkUpPro.Application.DTOs.User.Requests;
using LinkUpPro.Application.DTOs.User.Responses;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Models.Emails;
using LinkUpPro.Domain.Common;
using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace LinkUpPro.Infrastructure.Identity.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;

    public AccountService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IEmailService emailService,
        IFileService fileService,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _fileService = fileService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, bool rememberMe)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user is null)
            return AuthResponseDto.CreateError([
                "El nombre de usuario o la contrasena son incorrectos.",
            ]);

        if (!user.IsActive || !user.EmailConfirmed)
            return AuthResponseDto.CreateError([
                "Su cuenta se encuentra inactiva. Debe activarla mediante el enlace enviado a su correo electronico.",
            ]);

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            request.Password,
            rememberMe,
            lockoutOnFailure: true
        );

        if (result.IsLockedOut)
            return AuthResponseDto.CreateError([
                "La cuenta se encuentra bloqueada temporalmente debido a varios intentos fallidos. Intentelo nuevamente en 15 minutos o restablezca su contrasena.",
            ]);

        if (!result.Succeeded)
            return AuthResponseDto.CreateError([
                "El nombre de usuario o la contrasena son incorrectos.",
            ]);

        user.LastActivityAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);

        return AuthResponseDto.CreateSuccess(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.ProfilePicturePath,
            user.EmailConfirmed,
            roles.ToList()
        );
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, string origin)
    {
        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
            return AuthResponseDto.CreateError([
                "Este nombre de usuario ya se encuentra registrado.",
            ]);

        var existingEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmail is not null)
            return AuthResponseDto.CreateError([
                "Este correo electronico ya se encuentra registrado.",
            ]);

        var user = new AppUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            ProfilePicturePath = request.ProfilePicturePath,
            EmailConfirmed = false,
            IsActive = false,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return AuthResponseDto.CreateError(
                createResult.Errors.Select(e => e.Description).ToList()
            );

        await _userManager.AddToRoleAsync(user, "User");

        var verificationUri = await GenerateActivationUri(user, origin);
        await _emailService.SendEmailAsync(
            user.Email!,
            "Activa tu cuenta en LinkUp Pro",
            "AccountActivation",
            new ActivationEmailModel(user.UserName!, verificationUri)
        );

        return AuthResponseDto.CreateSuccess(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.ProfilePicturePath,
            false
        );
    }

    public async Task<AuthResponseDto> ConfirmAccountAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return AuthResponseDto.CreateError([
                "El enlace de activacion no es valido o ya fue utilizado.",
            ]);

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            return AuthResponseDto.CreateError([
                "El enlace de activacion no es valido o ya fue utilizado.",
            ]);

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        return AuthResponseDto.CreateSuccess(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.ProfilePicturePath,
            true
        );
    }

    public async Task<AuthResponseDto> ResendActivationAsync(ResendActivationRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is null || user.IsActive || user.EmailConfirmed)
            return AuthResponseDto.CreateSuccess(
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
            return AuthResponseDto.CreateError([
                "Debe esperar 5 minutos antes de solicitar un nuevo enlace de activacion.",
            ]);

        var verificationUri = await GenerateActivationUri(user, request.Origin);
        await _emailService.SendEmailAsync(
            user.Email!,
            "Activa tu cuenta en LinkUp Pro",
            "AccountActivation",
            new ActivationEmailModel(user.UserName!, verificationUri)
        );

        user.LastActivationEmailSentAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return AuthResponseDto.CreateSuccess(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            null,
            false
        );
    }

    public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is not null)
        {
            var resetUri = await GeneratePasswordResetUri(user, request.Origin);
            await _emailService.SendEmailAsync(
                user.Email!,
                "Restablece tu contrasena en LinkUp Pro",
                "PasswordReset",
                new ResetPasswordEmailModel(user.UserName!, resetUri)
            );
        }

        return AuthResponseDto.CreateSuccess(
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
            return AuthResponseDto.CreateError([
                "El enlace para restablecer la contrasena no es valido o ya fue utilizado.",
            ]);

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.Password);

        if (!result.Succeeded)
            return AuthResponseDto.CreateError([
                "El enlace para restablecer la contrasena no es valido o ya fue utilizado.",
            ]);

        await _userManager.UpdateSecurityStampAsync(user);

        return AuthResponseDto.CreateSuccess(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.ProfilePicturePath,
            true
        );
    }

    public async Task<UserProfileResponseDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new UserProfileResponseDto();

        return new UserProfileResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            ProfilePicturePath = user.ProfilePicturePath,
            IsActive = user.IsActive,
            IsVerified = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            LastActivityAt = user.LastActivityAt?.UtcDateTime,
        };
    }

    public async Task<EditProfileResponseDto> UpdateProfileAsync(
        string userId,
        UpdateProfileRequest request
    )
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return EditProfileResponseDto.CreateError(["Usuario no encontrado."]);

        string? oldPhotoPath = user.ProfilePicturePath;

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();

        if (request.ProfilePicturePath is not null)
            user.ProfilePicturePath = request.ProfilePicturePath;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return EditProfileResponseDto.CreateError(
                result.Errors.Select(e => e.Description).ToList()
            );

        if (
            request.ProfilePicturePath is not null
            && oldPhotoPath is not null
            && oldPhotoPath != "/images/default-avatar.png"
        )
            _fileService.DeleteFile(oldPhotoPath);

        return EditProfileResponseDto.CreateSuccess(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.EmailConfirmed
        );
    }

    public async Task<EditProfileResponseDto> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request
    )
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return EditProfileResponseDto.CreateError(["Usuario no encontrado."]);

        var passwordCheck = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordCheck)
            return EditProfileResponseDto.CreateError(["La contrasena actual es incorrecta."]);

        if (request.CurrentPassword == request.NewPassword)
            return EditProfileResponseDto.CreateError([
                "La nueva contrasena debe ser diferente de la contrasena actual.",
            ]);

        var changeResult = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword
        );
        if (!changeResult.Succeeded)
            return EditProfileResponseDto.CreateError(
                changeResult.Errors.Select(e => e.Description).ToList()
            );

        await _userManager.UpdateSecurityStampAsync(user);
        await _signInManager.SignOutAsync();

        return EditProfileResponseDto.CreateSuccessWithReLogin(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.EmailConfirmed
        );
    }

    public async Task<UserDto?> GetByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByUserNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : MapToDto(user);
    }

    public Task SignOutAsync()
    {
        return _signInManager.SignOutAsync();
    }

    private async Task<string> GenerateActivationUri(AppUser user, string origin)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var route = $"{origin}/Account/ConfirmAccount";
        var uri = QueryHelpers.AddQueryString(route, "userId", user.Id);
        uri = QueryHelpers.AddQueryString(uri, "token", encodedToken);
        return uri;
    }

    private async Task<string> GeneratePasswordResetUri(AppUser user, string origin)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var route = $"{origin}/Account/ResetPassword";
        var uri = QueryHelpers.AddQueryString(route, "userId", user.Id);
        uri = QueryHelpers.AddQueryString(uri, "token", encodedToken);
        return uri;
    }

    private static UserDto MapToDto(AppUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ProfilePicturePath = user.ProfilePicturePath,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
        };
    }
}
