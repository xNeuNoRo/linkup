using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Domain.Entities.Identity;

/// <summary>
/// Representa un usuario del sistema con propiedades 
/// y comportamientos relacionados con la identidad y autenticación.
/// </summary>
public sealed class User : AuditableBaseEntity<string>
{
    private User() { }

    public string UserName { get; private set; } = null!;

    public string NormalizedUserName { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string NormalizedEmail { get; private set; } = null!;

    public bool EmailConfirmed { get; private set; }

    public string PasswordHash { get; private set; } = null!;

    public PhoneNumber PhoneNumber { get; private set; } = null!;

    public bool PhoneNumberConfirmed { get; private set; }

    public bool LockoutEnabled { get; private set; }

    public DateTimeOffset? LockoutEnd { get; private set; }

    public int AccessFailedCount { get; private set; }

    public string FirstName { get; private set; } = null!;

    public string LastName { get; private set; } = null!;

    public string ProfilePicturePath { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTimeOffset? LastActivityAt { get; private set; }

    public static Result<User> Register(
        string userName,
        Email email,
        string passwordHash,
        string firstName,
        string lastName,
        PhoneNumber phoneNumber,
        string profilePicturePath,
        DateTimeOffset? createdAt = null
    )
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phoneNumber);

        var errors = ValidateRegistrationData(
            userName,
            passwordHash,
            firstName,
            lastName,
            profilePicturePath
        );

        if (errors.Count > 0)
        {
            return Result<User>.Failure(errors);
        }

        var trimmedUserName = userName.Trim();
        var normalizedEmail = email.Value.ToUpperInvariant();

        return Result<User>.Success(
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = trimmedUserName,
                NormalizedUserName = trimmedUserName.ToUpperInvariant(),
                Email = email.Value,
                NormalizedEmail = normalizedEmail,
                EmailConfirmed = false,
                PasswordHash = passwordHash,
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = false,
                LockoutEnabled = true,
                LockoutEnd = null,
                AccessFailedCount = 0,
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                ProfilePicturePath = profilePicturePath.Trim(),
                IsActive = false,
                LastActivityAt = null,
                CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            }
        );
    }

    public Result UpdateProfile(
        string firstName,
        string lastName,
        PhoneNumber phoneNumber,
        string? newProfilePicturePath = null,
        DateTimeOffset? updatedAt = null
    )
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        var errors = new List<DomainError>();
        AddNameErrors(
            errors,
            firstName,
            nameof(FirstName),
            "User.FirstNameRequired",
            "Debe ingresar su nombre."
        );
        AddNameErrors(
            errors,
            lastName,
            nameof(LastName),
            "User.LastNameRequired",
            "Debe ingresar su apellido."
        );

        if (newProfilePicturePath is not null)
        {
            AddProfilePicturePathErrors(errors, newProfilePicturePath);
        }

        if (errors.Count > 0)
        {
            return Result.Failure(errors);
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber;

        if (newProfilePicturePath is not null)
        {
            ProfilePicturePath = newProfilePicturePath.Trim();
        }

        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result ChangePassword(
        string currentPasswordHash,
        string newPasswordHash,
        DateTimeOffset? updatedAt = null
    )
    {
        if (string.IsNullOrWhiteSpace(currentPasswordHash) || currentPasswordHash != PasswordHash)
        {
            return Result.Failure(
                new DomainError(
                    "User.CurrentPasswordInvalid",
                    "La contrasena actual es incorrecta."
                )
            );
        }

        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            return Result.Failure(
                new DomainError("User.NewPasswordRequired", "La nueva contrasena es requerida.")
            );
        }

        if (newPasswordHash == PasswordHash)
        {
            return Result.Failure(
                new DomainError(
                    "User.NewPasswordMustBeDifferent",
                    "La nueva contrasena debe ser diferente de la contrasena actual."
                )
            );
        }

        PasswordHash = newPasswordHash;
        ResetFailedLogins(updatedAt);
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void ActivateAccount(DateTimeOffset? updatedAt = null)
    {
        IsActive = true;
        EmailConfirmed = true;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    public void DeactivateAccount(DateTimeOffset? updatedAt = null)
    {
        IsActive = false;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    public void UpdateLastActivity(DateTimeOffset? activityAt = null)
    {
        LastActivityAt = activityAt ?? DateTimeOffset.UtcNow;
    }

    public bool IsLockedOut(DateTimeOffset? now = null)
    {
        var referenceTime = now ?? DateTimeOffset.UtcNow;
        return LockoutEnd.HasValue && LockoutEnd.Value > referenceTime;
    }

    public bool CanLogin(DateTimeOffset? now = null) =>
        IsActive && EmailConfirmed && !IsLockedOut(now);

    public bool IsSessionInactive(DateTimeOffset now) =>
        LastActivityAt.HasValue
        && now - LastActivityAt.Value >= DomainConstants.SessionInactivityTimeout;

    public void RecordFailedLogin(DateTimeOffset? failedAt = null)
    {
        var referenceTime = failedAt ?? DateTimeOffset.UtcNow;

        if (!LockoutEnabled || IsLockedOut(referenceTime))
        {
            return;
        }

        AccessFailedCount++;

        if (AccessFailedCount >= DomainConstants.MaxFailedAccessAttempts)
        {
            LockoutEnd = referenceTime.Add(DomainConstants.LoginLockoutDuration);
        }
    }

    public void ResetFailedLogins(DateTimeOffset? updatedAt = null)
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    public string GetDisplayName() => $"{FirstName} {LastName}";

    public string GetInitials()
    {
        var firstInitial = FirstName.Trim()[0];
        var lastInitial = LastName.Trim()[0];

        return string.Create(
            2,
            (firstInitial, lastInitial),
            static (span, initials) =>
            {
                span[0] = char.ToUpperInvariant(initials.firstInitial);
                span[1] = char.ToUpperInvariant(initials.lastInitial);
            }
        );
    }

    private static List<DomainError> ValidateRegistrationData(
        string userName,
        string passwordHash,
        string firstName,
        string lastName,
        string profilePicturePath
    )
    {
        var errors = new List<DomainError>();

        AddUserNameErrors(errors, userName);
        AddPasswordHashErrors(errors, passwordHash);
        AddNameErrors(
            errors,
            firstName,
            nameof(FirstName),
            "User.FirstNameRequired",
            "Debe ingresar su nombre."
        );
        AddNameErrors(
            errors,
            lastName,
            nameof(LastName),
            "User.LastNameRequired",
            "Debe ingresar su apellido."
        );
        AddProfilePicturePathErrors(errors, profilePicturePath);

        return errors;
    }

    private static void AddUserNameErrors(List<DomainError> errors, string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            errors.Add(
                new DomainError("User.UserNameRequired", "El nombre de usuario es requerido.")
            );
            return;
        }

        if (userName.Trim().Length > DomainConstants.MaxUserNameLength)
        {
            errors.Add(
                new DomainError(
                    "User.UserNameTooLong",
                    "El nombre de usuario excede la longitud permitida."
                )
            );
        }
    }

    private static void AddPasswordHashErrors(List<DomainError> errors, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            errors.Add(new DomainError("User.PasswordHashRequired", "La contrasena es requerida."));
        }
    }

    private static void AddNameErrors(
        List<DomainError> errors,
        string value,
        string propertyName,
        string requiredCode,
        string requiredMessage
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new DomainError(requiredCode, requiredMessage));
            return;
        }

        var maxLength =
            propertyName == nameof(FirstName)
                ? DomainConstants.MaxUserFirstNameLength
                : DomainConstants.MaxUserLastNameLength;

        if (value.Trim().Length > maxLength)
        {
            errors.Add(
                new DomainError(
                    $"User.{propertyName}TooLong",
                    $"{propertyName} excede la longitud permitida."
                )
            );
        }
    }

    private static void AddProfilePicturePathErrors(
        List<DomainError> errors,
        string profilePicturePath
    )
    {
        if (string.IsNullOrWhiteSpace(profilePicturePath))
        {
            errors.Add(
                new DomainError("User.ProfilePictureRequired", "La foto de perfil es requerida.")
            );
            return;
        }

        if (profilePicturePath.Trim().Length > DomainConstants.MaxProfilePicturePathLength)
        {
            errors.Add(
                new DomainError(
                    "User.ProfilePicturePathTooLong",
                    "La ruta de la foto de perfil excede la longitud permitida."
                )
            );
        }
    }
}
