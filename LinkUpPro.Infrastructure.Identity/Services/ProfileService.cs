using LinkUpPro.Application.DTOs.Profile.Requests;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.ValueObjects;
using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Identity.Services;

public sealed class ProfileService : IProfileService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProfileService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IFileService fileService,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _fileService = fileService;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<UserProfileResponseDto>> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<UserProfileResponseDto>.Failure(
                new DomainError("User.NotFound", "El usuario no fue encontrado.")
            );

        return Result<UserProfileResponseDto>.Success(MapToProfileDto(user));
    }

    public async Task<Result<EditProfileResponseDto>> UpdateProfileAsync(
        string userId,
        UpdateProfileRequest request
    )
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("User.NotFound", "El usuario no fue encontrado.")
            );

        PhoneNumber? phone = null;
        try
        {
            phone = PhoneNumber.Create(request.PhoneNumber);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result<EditProfileResponseDto>.Failure(new DomainError(ex.Code, ex.Message));
        }

        var oldPhoto = user.ProfilePicturePath;

        var firstName = request.FirstName?.Trim() ?? string.Empty;
        var lastName = request.LastName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName))
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("Profile.FirstNameRequired", "El nombre es requerido y no puede contener solo espacios.")
            );

        if (string.IsNullOrWhiteSpace(lastName))
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("Profile.LastNameRequired", "El apellido es requerido y no puede contener solo espacios.")
            );

        user.FirstName = firstName;
        user.LastName = lastName;
        user.SetPhoneNumber(phone.Value);

        if (request.ProfilePictureFile is not null && request.ProfilePictureFile.Length > 0)
        {
            if (!_fileService.IsImageValid(request.ProfilePictureFile))
                return Result<EditProfileResponseDto>.Failure(
                    new DomainError(
                        "Profile.InvalidImage",
                        "El archivo seleccionado no tiene un formato de imagen válido o supera los 5 MB."
                    )
                );

            user.ProfilePicturePath = await _fileService.UploadFileAsync(
                request.ProfilePictureFile,
                "profiles"
            );
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var error = updateResult.Errors.FirstOrDefault();
            return Result<EditProfileResponseDto>.Failure(
                new DomainError(
                    "Profile.UpdateFailed",
                    error?.Description ?? "Error al actualizar el perfil."
                )
            );
        }

        await _unitOfWork.SaveChangesAsync();

        if (
            request.ProfilePictureFile is not null
            && request.ProfilePictureFile.Length > 0
            && !string.IsNullOrEmpty(oldPhoto)
        )
        {
            _fileService.DeleteFile(oldPhoto);
        }

        // Refrescar la cookie con los nuevos claims (FirstName, LastName, ProfilePicturePath)
        await RefreshSignInCookieAsync(user);

        return Result<EditProfileResponseDto>.Success(MapToEditDto(user));
    }

    private async Task RefreshSignInCookieAsync(AppUser user)
    {
        try
        {
            var roles = await _userManager.GetRolesAsync(user);
            var rememberMeClaim = _httpContextAccessor
                .HttpContext?
                .User.FindFirst("RememberMe")?.Value;
            var rememberMe = bool.TryParse(rememberMeClaim, out var rm) && rm;

            var claims = new List<System.Security.Claims.Claim>
            {
                new("FirstName", user.FirstName ?? string.Empty),
                new("LastName", user.LastName ?? string.Empty),
                new("ProfilePicturePath", user.ProfilePicturePath ?? string.Empty),
                new("RememberMe", rememberMe.ToString()),
                new("LastActivityAt", DateTimeOffset.UtcNow.ToString("O")),
            };

            foreach (var role in roles)
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
            }

            await _signInManager.SignInWithClaimsAsync(user, rememberMe, claims);
        }
        catch
        {
            // Si no se puede refrescar la cookie (ej: tests, contexto sin auth),
            // no afecta la actualización del perfil. Los claims se actualizarán en el próximo login.
        }
    }

    public async Task<Result<EditProfileResponseDto>> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request
    )
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("User.NotFound", "El usuario no fue encontrado.")
            );

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("Password.CurrentRequired", "La contrasena actual es requerida.")
            );

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("Password.NewRequired", "La nueva contrasena es requerida.")
            );

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("Password.ConfirmRequired", "La confirmacion de contrasena es requerida.")
            );

        if (request.NewPassword != request.ConfirmPassword)
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("Password.Mismatch", "La nueva contrasena y su confirmacion no coinciden.")
            );

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid)
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("Password.Invalid", "La contrasena actual es incorrecta.")
            );

        if (request.CurrentPassword == request.NewPassword)
            return Result<EditProfileResponseDto>.Failure(
                new DomainError("Password.SameAsCurrent", "La nueva contrasena no puede ser igual a la actual.")
            );

        var strength = PasswordStrength.Calculate(request.NewPassword);
        if (!strength.IsStrong)
        {
            var missing = new List<string>();
            if ((strength.CriteriaMet & PasswordStrengthCriteria.MinimumLength) == 0)
                missing.Add("al menos 8 caracteres");
            if ((strength.CriteriaMet & PasswordStrengthCriteria.UppercaseLetter) == 0)
                missing.Add("una mayuscula");
            if ((strength.CriteriaMet & PasswordStrengthCriteria.LowercaseLetter) == 0)
                missing.Add("una minuscula");
            if ((strength.CriteriaMet & PasswordStrengthCriteria.Digit) == 0)
                missing.Add("un numero");
            if ((strength.CriteriaMet & PasswordStrengthCriteria.SpecialCharacter) == 0)
                missing.Add("un caracter especial");

            return Result<EditProfileResponseDto>.Failure(
                new DomainError(
                    "Password.Weak",
                    $"La nueva contrasena debe contener: {string.Join(", ", missing)}."
                )
            );
        }

        var changeResult = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword
        );
        if (!changeResult.Succeeded)
        {
            var error = changeResult.Errors.FirstOrDefault();
            return Result<EditProfileResponseDto>.Failure(
                new DomainError(
                    "Password.ChangeFailed",
                    error?.Description ?? "Error al cambiar la contrasena."
                )
            );
        }

        await _userManager.UpdateSecurityStampAsync(user);
        await _signInManager.SignOutAsync();
        await _unitOfWork.SaveChangesAsync();

        return Result<EditProfileResponseDto>.Success(
            MapToEditDto(user) with
            {
                RequiresReLogin = true,
            }
        );
    }

    public async Task<UserResponseDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user is null ? null : MapToUserResponseDto(user);
    }

    public async Task<IReadOnlyDictionary<string, UserResponseDto>> GetByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default
    )
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new Dictionary<string, UserResponseDto>();

        var users = await _userManager
            .Users.Where(u => idList.Contains(u.Id))
            .Select(u => new UserResponseDto(
                u.Id,
                u.UserName ?? string.Empty,
                u.Email ?? string.Empty,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.ProfilePicturePath,
                u.IsActive,
                u.EmailConfirmed
            ))
            .ToListAsync(cancellationToken);

        return users.ToDictionary(u => u.Id);
    }

    public async Task<UserResponseDto?> GetByUserNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user is null ? null : MapToUserResponseDto(user);
    }

    public async Task<UserResponseDto?> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : MapToUserResponseDto(user);
    }

    private static UserProfileResponseDto MapToProfileDto(AppUser user) =>
        new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.PhoneNumber ?? string.Empty,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.ProfilePicturePath,
            user.IsActive,
            user.EmailConfirmed,
            user.CreatedAt,
            user.LastActivityAt?.DateTime
        );

    private static EditProfileResponseDto MapToEditDto(AppUser user) =>
        new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.EmailConfirmed,
            false
        );

    private static UserResponseDto MapToUserResponseDto(AppUser user) =>
        new(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.ProfilePicturePath,
            user.IsActive,
            user.EmailConfirmed
        );
}
