using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Identity;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.Entities.Identity;

public class UserTests
{
    [Fact]
    public void Register_ValidData_CreatesInactiveUserWithIdentityDefaults()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        // Act
        var result = CreateUser(createdAt: createdAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
        Assert.False(result.Value.EmailConfirmed);
        Assert.False(result.Value.PhoneNumberConfirmed);
        Assert.True(result.Value.LockoutEnabled);
        Assert.Equal(0, result.Value.AccessFailedCount);
        Assert.Null(result.Value.LockoutEnd);
        Assert.Equal(createdAt, result.Value.CreatedAt);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.Id));
    }

    [Fact]
    public void Register_ValidData_NormalizesUserNameAndEmail()
    {
        // Arrange & Act
        var result = User.Register(
            "  angelUser ",
            Email.Create("  Angel@Test.COM "),
            "HASHED_PASSWORD",
            "Angel",
            "Perez",
            PhoneNumber.Create("809-555-1234"),
            "/uploads/profile.webp");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("angelUser", result.Value.UserName);
        Assert.Equal("ANGELUSER", result.Value.NormalizedUserName);
        Assert.Equal("angel@test.com", result.Value.Email);
        Assert.Equal("ANGEL@TEST.COM", result.Value.NormalizedEmail);
    }

    [Fact]
    public void Register_MissingRequiredFields_ReturnsValidationErrors()
    {
        // Arrange & Act
        var result = User.Register(
            " ",
            Email.Create("user@test.com"),
            " ",
            " ",
            " ",
            PhoneNumber.Create("809-555-1234"),
            " ");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "User.UserNameRequired");
        Assert.Contains(result.Errors, error => error.Code == "User.PasswordHashRequired");
        Assert.Contains(result.Errors, error => error.Code == "User.FirstNameRequired");
        Assert.Contains(result.Errors, error => error.Code == "User.LastNameRequired");
        Assert.Contains(result.Errors, error => error.Code == "User.ProfilePictureRequired");
    }

    [Fact]
    public void Register_TooLongFields_ReturnsValidationErrors()
    {
        // Arrange
        var tooLongUserName = new string('u', DomainConstants.MaxUserNameLength + 1);
        var tooLongFirstName = new string('f', DomainConstants.MaxUserFirstNameLength + 1);
        var tooLongLastName = new string('l', DomainConstants.MaxUserLastNameLength + 1);
        var tooLongProfilePath = new string('p', DomainConstants.MaxProfilePicturePathLength + 1);

        // Act
        var result = User.Register(
            tooLongUserName,
            Email.Create("user@test.com"),
            "HASHED_PASSWORD",
            tooLongFirstName,
            tooLongLastName,
            PhoneNumber.Create("809-555-1234"),
            tooLongProfilePath);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "User.UserNameTooLong");
        Assert.Contains(result.Errors, error => error.Code == "User.FirstNameTooLong");
        Assert.Contains(result.Errors, error => error.Code == "User.LastNameTooLong");
        Assert.Contains(result.Errors, error => error.Code == "User.ProfilePicturePathTooLong");
    }

    [Fact]
    public void ActivateAccount_InactiveUser_AllowsLogin()
    {
        // Arrange
        var user = CreateValidUser();
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        // Act
        user.ActivateAccount(now);

        // Assert
        Assert.True(user.IsActive);
        Assert.True(user.EmailConfirmed);
        Assert.True(user.CanLogin(now));
        Assert.Equal(now, user.UpdatedAt);
    }

    [Fact]
    public void CanLogin_InactiveUser_ReturnsFalse()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var canLogin = user.CanLogin();

        // Assert
        Assert.False(canLogin);
    }

    [Fact]
    public void DeactivateAccount_ActiveUser_PreventsLogin()
    {
        // Arrange
        var user = CreateValidUser();
        user.ActivateAccount();

        // Act
        user.DeactivateAccount();

        // Assert
        Assert.False(user.IsActive);
        Assert.False(user.CanLogin());
    }

    [Fact]
    public void RecordFailedLogin_FifthAttempt_LocksForFifteenMinutes()
    {
        // Arrange
        var user = CreateValidUser();
        var failedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        // Act
        for (var attempt = 0; attempt < DomainConstants.MaxFailedAccessAttempts; attempt++)
        {
            user.RecordFailedLogin(failedAt);
        }

        // Assert
        Assert.Equal(DomainConstants.MaxFailedAccessAttempts, user.AccessFailedCount);
        Assert.Equal(failedAt.Add(DomainConstants.LoginLockoutDuration), user.LockoutEnd);
        Assert.True(user.IsLockedOut(failedAt.AddMinutes(1)));
    }

    [Fact]
    public void RecordFailedLogin_WhenLocked_DoesNotExtendLockout()
    {
        // Arrange
        var user = CreateValidUser();
        var failedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        for (var attempt = 0; attempt < DomainConstants.MaxFailedAccessAttempts; attempt++)
        {
            user.RecordFailedLogin(failedAt);
        }

        var originalLockoutEnd = user.LockoutEnd;

        // Act
        user.RecordFailedLogin(failedAt.AddMinutes(5));

        // Assert
        Assert.Equal(originalLockoutEnd, user.LockoutEnd);
        Assert.Equal(DomainConstants.MaxFailedAccessAttempts, user.AccessFailedCount);
    }

    [Fact]
    public void ResetFailedLogins_LockedUser_ClearsLockoutAndFailedCount()
    {
        // Arrange
        var user = CreateValidUser();
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        for (var attempt = 0; attempt < DomainConstants.MaxFailedAccessAttempts; attempt++)
        {
            user.RecordFailedLogin(now);
        }

        // Act
        user.ResetFailedLogins(now.AddMinutes(1));

        // Assert
        Assert.Equal(0, user.AccessFailedCount);
        Assert.Null(user.LockoutEnd);
        Assert.Equal(now.AddMinutes(1), user.UpdatedAt);
    }

    [Fact]
    public void UpdateProfile_ValidData_UpdatesEditableFields()
    {
        // Arrange
        var user = CreateValidUser();
        var updatedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var newPhone = PhoneNumber.Create("829-555-1234");

        // Act
        var result = user.UpdateProfile(
            "  Ana ",
            "  Gomez ",
            newPhone,
            "/uploads/new.webp",
            updatedAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Ana", user.FirstName);
        Assert.Equal("Gomez", user.LastName);
        Assert.Equal(newPhone, user.PhoneNumber);
        Assert.Equal("/uploads/new.webp", user.ProfilePicturePath);
        Assert.Equal(updatedAt, user.UpdatedAt);
    }

    [Fact]
    public void UpdateProfile_WithoutNewPhoto_PreservesExistingPhoto()
    {
        // Arrange
        var user = CreateValidUser();
        var originalPhoto = user.ProfilePicturePath;

        // Act
        var result = user.UpdateProfile("Ana", "Gomez", PhoneNumber.Create("829-555-1234"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalPhoto, user.ProfilePicturePath);
    }

    [Fact]
    public void UpdateProfile_MissingNames_ReturnsValidationErrors()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var result = user.UpdateProfile(" ", " ", PhoneNumber.Create("829-555-1234"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "User.FirstNameRequired");
        Assert.Contains(result.Errors, error => error.Code == "User.LastNameRequired");
    }

    [Fact]
    public void ChangePassword_WithValidCurrentHash_UpdatesPasswordAndResetsFailures()
    {
        // Arrange
        var user = CreateValidUser();
        user.RecordFailedLogin();
        var updatedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        // Act
        var result = user.ChangePassword("HASHED_PASSWORD", "NEW_HASHED_PASSWORD", updatedAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("NEW_HASHED_PASSWORD", user.PasswordHash);
        Assert.Equal(0, user.AccessFailedCount);
        Assert.Null(user.LockoutEnd);
        Assert.Equal(updatedAt, user.UpdatedAt);
    }

    [Fact]
    public void ChangePassword_WithInvalidCurrentHash_ReturnsFailure()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var result = user.ChangePassword("WRONG_HASH", "NEW_HASHED_PASSWORD");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.CurrentPasswordInvalid", result.Error.Code);
        Assert.Equal("HASHED_PASSWORD", user.PasswordHash);
    }

    [Fact]
    public void ChangePassword_WithSameHash_ReturnsFailure()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var result = user.ChangePassword("HASHED_PASSWORD", "HASHED_PASSWORD");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.NewPasswordMustBeDifferent", result.Error.Code);
    }

    [Fact]
    public void UpdateLastActivity_SetsLastActivityAt()
    {
        // Arrange
        var user = CreateValidUser();
        var activityAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        // Act
        user.UpdateLastActivity(activityAt);

        // Assert
        Assert.Equal(activityAt, user.LastActivityAt);
    }

    [Fact]
    public void IsSessionInactive_AfterThirtyMinutes_ReturnsTrue()
    {
        // Arrange
        var user = CreateValidUser();
        var activityAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        user.UpdateLastActivity(activityAt);

        // Act
        var inactive = user.IsSessionInactive(activityAt.Add(DomainConstants.SessionInactivityTimeout));

        // Assert
        Assert.True(inactive);
    }

    [Fact]
    public void GetDisplayName_AndGetInitials_ReturnExpectedValues()
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Equal("Angel Perez", user.GetDisplayName());
        Assert.Equal("AP", user.GetInitials());
    }

    private static User CreateValidUser() => CreateUser().Value;

    private static Result<User> CreateUser(DateTimeOffset? createdAt = null) => User.Register(
        "angel",
        Email.Create("angel@test.com"),
        "HASHED_PASSWORD",
        "Angel",
        "Perez",
        PhoneNumber.Create("809-555-1234"),
        "/uploads/profile.webp",
        createdAt);
}
