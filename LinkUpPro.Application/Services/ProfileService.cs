using System.Text.RegularExpressions;
using LinkUpPro.Application.DTOs.Profile.Requests;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Exceptions;

namespace LinkUpPro.Application.Services;

public sealed class ProfileService : IProfileService
{
    private readonly dynamic _userManager;
    private readonly dynamic _signInManager;
    private readonly IEmailService? _emailService;
    private readonly IFileService? _fileService;

    public ProfileService(
        dynamic userManager,
        dynamic signInManager,
        IEmailService? emailService,
        IFileService? fileService,
        object? configuration = null)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _fileService = fileService;
    }

    public async Task<UserProfileResponseDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new DomainValidationException("UserId", "El usuario no fue encontrado.", "User.NotFound");

        return MapToProfileDto(user);
    }

    public async Task<EditProfileResponseDto> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new DomainValidationException("UserId", "El usuario no fue encontrado.", "User.NotFound");

        if (!string.IsNullOrEmpty(request.PhoneNumber) &&
            !Regex.IsMatch(request.PhoneNumber, @"^\d{3}-\d{3}-\d{4}$"))
        {
            throw new DomainValidationException("PhoneNumber", "El numero de telefono no es valido.", "User.InvalidPhone");
        }

        var oldPhoto = user.ProfilePicturePath as string;

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        if (request.ProfilePicturePath is not null)
            user.ProfilePicturePath = request.ProfilePicturePath;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var error = updateResult.Errors is not null
                ? ((IEnumerable<dynamic>)updateResult.Errors).FirstOrDefault()
                : null;
            throw new DomainValidationException(
                "Profile",
                error?.Description ?? "Error al actualizar el perfil.",
                "Profile.UpdateFailed");
        }

        if (request.ProfilePicturePath is not null &&
            oldPhoto is not null &&
            !((string)oldPhoto).Contains("default"))
        {
            _fileService?.DeleteFile((string)oldPhoto);
        }

        return new EditProfileResponseDto(
            (string)user.Id,
            (string)user.FirstName,
            (string)user.LastName,
            (string)(user.Email ?? string.Empty),
            (string)(user.UserName ?? string.Empty),
            (bool)user.EmailConfirmed,
            false
        );
    }

    public async Task<EditProfileResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new DomainValidationException("UserId", "El usuario no fue encontrado.", "User.NotFound");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid)
            throw new DomainValidationException("CurrentPassword", "La contrasena actual es incorrecta.", "Password.Invalid");

        if (request.CurrentPassword == request.NewPassword)
            throw new DomainValidationException("NewPassword", "La nueva contrasena no puede ser igual a la actual.", "Password.SameAsCurrent");

        var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
        {
            var error = changeResult.Errors is not null
                ? ((IEnumerable<dynamic>)changeResult.Errors).FirstOrDefault()
                : null;
            throw new DomainValidationException(
                "CurrentPassword",
                error?.Description ?? "La contrasena actual es incorrecta.",
                "Password.Invalid");
        }

        await _userManager.UpdateSecurityStampAsync(user);
        await _signInManager.SignOutAsync();

        return new EditProfileResponseDto(
            (string)user.Id,
            (string)user.FirstName,
            (string)user.LastName,
            (string)(user.Email ?? string.Empty),
            (string)(user.UserName ?? string.Empty),
            (bool)user.EmailConfirmed,
            RequiresReLogin: true
        );
    }

    public async Task<UserResponseDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user is null ? null : MapToUserResponseDto(user);
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

    private static UserProfileResponseDto MapToProfileDto(dynamic user) =>
        new(
            (string)user.Id,
            (string)user.FirstName,
            (string)user.LastName,
            (string)(user.PhoneNumber ?? string.Empty),
            (string)(user.Email ?? string.Empty),
            (string)(user.UserName ?? string.Empty),
            (string?)user.ProfilePicturePath,
            (bool)user.IsActive,
            (bool)user.EmailConfirmed,
            (DateTime)user.CreatedAt,
            user.LastActivityAt?.DateTime
        );

    private static UserResponseDto MapToUserResponseDto(dynamic user) =>
        new(
            (string)user.Id,
            (string)(user.UserName ?? string.Empty),
            (string)(user.Email ?? string.Empty),
            (string)user.FirstName,
            (string)user.LastName,
            (string?)user.PhoneNumber,
            (string?)user.ProfilePicturePath,
            (bool)user.IsActive,
            (bool)user.EmailConfirmed
        );
}
