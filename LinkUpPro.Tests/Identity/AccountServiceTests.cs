using LinkUpPro.Application.DTOs.Profile.Requests;
using LinkUpPro.Application.DTOs.User.Requests;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Models.Emails;
using LinkUpPro.Infrastructure.Identity.Entities;
using LinkUpPro.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LinkUpPro.Tests.Identity;

public class AccountServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<SignInManager<AppUser>> _signInManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        _userManagerMock = MockUserManager();
        _signInManagerMock = MockSignInManager();
        _emailServiceMock = new Mock<IEmailService>();
        _fileServiceMock = new Mock<IFileService>();

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c[It.IsAny<string>()]).Returns("test");

        _sut = new AccountService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _emailServiceMock.Object,
            _fileServiceMock.Object,
            configMock.Object
        );
    }

    // ==================== LOGIN TESTS ====================

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["User"]);
        _signInManagerMock
            .Setup(m => m.PasswordSignInAsync("testuser", "Pass123!", true, true))
            .ReturnsAsync(SignInResult.Success);

        var result = await _sut.LoginAsync(new LoginRequest("testuser", "Pass123!"), true);

        result.HasError.Should().BeFalse();
        result.UserName.Should().Be("testuser");
        result.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsGenericError()
    {
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync((AppUser?)null);

        var result = await _sut.LoginAsync(new LoginRequest("testuser", "wrong"), false);

        result.HasError.Should().BeTrue();
        result
            .Errors.Should()
            .Contain(e => e.Contains("nombre de usuario o la contrasena son incorrectos"));
    }

    [Fact]
    public async Task LoginAsync_LockedOut_ReturnsLockoutMessage()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync(user);
        _signInManagerMock
            .Setup(m => m.PasswordSignInAsync("testuser", "Pass123!", false, true))
            .ReturnsAsync(SignInResult.LockedOut);

        var result = await _sut.LoginAsync(new LoginRequest("testuser", "Pass123!"), false);

        result.HasError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("bloqueada temporalmente"));
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ReturnsInactiveMessage()
    {
        var user = CreateInactiveUser();
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginRequest("testuser", "Pass123!"), false);

        result.HasError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("cuenta se encuentra inactiva"));
    }

    // ==================== REGISTER TESTS ====================

    [Fact]
    public async Task RegisterAsync_ValidData_CreatesUserAndSendsEmail()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByNameAsync("newuser")).ReturnsAsync((AppUser?)null);
        _userManagerMock
            .Setup(m => m.FindByEmailAsync("new@test.com"))
            .ReturnsAsync((AppUser?)null);
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), "Pass123!"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<AppUser, string>((u, _) => u.Id = "new-id");
        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>()))
            .ReturnsAsync("valid-token");
        _emailServiceMock
            .Setup(m =>
                m.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEmailModel>(),
                    default
                )
            )
            .ReturnsAsync(true);

        var request = new RegisterRequest(
            "newuser",
            "new@test.com",
            "Pass123!",
            "Pass123!",
            "John",
            "Doe",
            "809-555-1234",
            "/images/avatar.jpg"
        );

        var result = await _sut.RegisterAsync(request, "http://localhost");

        result.HasError.Should().BeFalse();
        result.UserName.Should().Be("newuser");
        result.IsVerified.Should().BeFalse();
        _emailServiceMock.Verify(
            m =>
                m.SendEmailAsync(
                    "new@test.com",
                    It.IsAny<string>(),
                    "AccountActivation",
                    It.IsAny<ActivationEmailModel>(),
                    default
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsError()
    {
        _userManagerMock.Setup(m => m.FindByNameAsync("existing")).ReturnsAsync(new AppUser());

        var request = new RegisterRequest(
            "existing",
            "new@test.com",
            "Pass123!",
            "Pass123!",
            "John",
            "Doe",
            "809-555-1234",
            "/images/avatar.jpg"
        );

        var result = await _sut.RegisterAsync(request, "http://localhost");

        result.HasError.Should().BeTrue();
        result
            .Errors.Should()
            .Contain(e => e.Contains("nombre de usuario ya se encuentra registrado"));
    }

    // ==================== ACTIVATION TESTS ====================

    [Fact]
    public async Task ConfirmAccountAsync_ValidToken_ActivatesUser()
    {
        var user = CreateInactiveUser();
        var rawToken = "test-confirm-token";
        var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(
            System.Text.Encoding.UTF8.GetBytes(rawToken)
        );
        _userManagerMock.Setup(m => m.FindByIdAsync("user1")).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.ConfirmEmailAsync(user, rawToken))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ConfirmAccountAsync("user1", encodedToken);

        result.HasError.Should().BeFalse();
        result.IsVerified.Should().BeTrue();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAccountAsync_InvalidToken_ReturnsError()
    {
        var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(
            System.Text.Encoding.UTF8.GetBytes("some-token")
        );
        _userManagerMock.Setup(m => m.FindByIdAsync("user1")).ReturnsAsync((AppUser?)null);

        var result = await _sut.ConfirmAccountAsync("user1", encodedToken);

        result.HasError.Should().BeTrue();
    }

    // ==================== FORGOT/RESET PASSWORD TESTS ====================

    [Fact]
    public async Task ForgotPasswordAsync_Always_ReturnsGenericSuccess()
    {
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync((AppUser?)null);

        var result = await _sut.ForgotPasswordAsync(
            new ForgotPasswordRequest("testuser", "http://localhost")
        );

        result.HasError.Should().BeFalse();
        _emailServiceMock.Verify(
            m =>
                m.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEmailModel>(),
                    default
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ForgotPasswordAsync_ExistingUser_SendsEmail()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        var result = await _sut.ForgotPasswordAsync(
            new ForgotPasswordRequest("testuser", "http://localhost")
        );

        result.HasError.Should().BeFalse();
        _emailServiceMock.Verify(
            m =>
                m.SendEmailAsync(
                    "test@test.com",
                    It.IsAny<string>(),
                    "PasswordReset",
                    It.IsAny<ResetPasswordEmailModel>(),
                    default
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_UpdatesPassword()
    {
        var user = CreateActiveUser();
        var rawToken = "reset-token-123";
        var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(
            System.Text.Encoding.UTF8.GetBytes(rawToken)
        );
        _userManagerMock.Setup(m => m.FindByIdAsync("user1")).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.ResetPasswordAsync(user, rawToken, "NewPass123!"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ResetPasswordAsync(
            new ResetPasswordRequest("user1", encodedToken, "NewPass123!", "NewPass123!")
        );

        result.HasError.Should().BeFalse();
        _userManagerMock.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
    }

    // ==================== PROFILE TESTS ====================

    [Fact]
    public async Task GetProfileAsync_ExistingUser_ReturnsProfile()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByIdAsync("user1")).ReturnsAsync(user);

        var result = await _sut.GetProfileAsync("user1");

        result.UserName.Should().Be("testuser");
        result.FirstName.Should().Be("John");
        result.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task GetProfileAsync_NonexistentUser_ReturnsEmpty()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("nobody")).ReturnsAsync((AppUser?)null);

        var result = await _sut.GetProfileAsync("nobody");

        result.UserName.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidData_UpdatesUser()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByIdAsync("user1")).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var request = new UpdateProfileRequest("Jane", "Smith", "829-555-5678", null);

        var result = await _sut.UpdateProfileAsync("user1", request);

        result.HasError.Should().BeFalse();
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        user.FirstName.Should().Be("Jane");
        user.PhoneNumber.Should().Be("829-555-5678");
    }

    // ==================== CHANGE PASSWORD TESTS ====================

    [Fact]
    public async Task ChangePasswordAsync_CorrectCurrent_ChangesPasswordAndLogsOut()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByIdAsync("user1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "OldPass1!")).ReturnsAsync(true);
        _userManagerMock
            .Setup(m => m.ChangePasswordAsync(user, "OldPass1!", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ChangePasswordAsync(
            "user1",
            new ChangePasswordRequest("OldPass1!", "NewPass1!", "NewPass1!")
        );

        result.HasError.Should().BeFalse();
        result.RequiresReLogin.Should().BeTrue();
        _userManagerMock.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
        _signInManagerMock.Verify(m => m.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrent_ReturnsError()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByIdAsync("user1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "WrongPass1!")).ReturnsAsync(false);

        var result = await _sut.ChangePasswordAsync(
            "user1",
            new ChangePasswordRequest("WrongPass1!", "NewPass1!", "NewPass1!")
        );

        result.HasError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("contrasena actual es incorrecta"));
    }

    // ==================== LOOKUP TESTS ====================

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ReturnsDto()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync("test@test.com")).ReturnsAsync(user);

        var result = await _sut.GetByEmailAsync("test@test.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task GetByUserNameAsync_NonexistentUser_ReturnsNull()
    {
        _userManagerMock.Setup(m => m.FindByNameAsync("nobody")).ReturnsAsync((AppUser?)null);

        var result = await _sut.GetByUserNameAsync("nobody");

        result.Should().BeNull();
    }

    // ==================== HELPER METHODS ====================

    private static AppUser CreateActiveUser()
    {
        return new AppUser
        {
            Id = "user1",
            UserName = "testuser",
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "809-555-1234",
            ProfilePicturePath = "/images/default-avatar.png",
            EmailConfirmed = true,
            IsActive = true,
        };
    }

    private static AppUser CreateInactiveUser()
    {
        var user = CreateActiveUser();
        user.EmailConfirmed = false;
        user.IsActive = false;
        return user;
    }

    private static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
    }

    private Mock<SignInManager<AppUser>> MockSignInManager()
    {
        var ctx = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
        return new Mock<SignInManager<AppUser>>(
            _userManagerMock.Object,
            ctx.Object,
            claimsFactory.Object,
            null!,
            null!,
            null!,
            null!
        );
    }
}
