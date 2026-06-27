using LinkUpPro.Application.DTOs.User.Requests;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.Models.Emails;
using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Infrastructure.Identity.Entities;
using LinkUpPro.Infrastructure.Identity.Mappings;
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
    private readonly Mock<IProfileService> _profileServiceMock;
    private readonly AccountService _sut;

    static AccountServiceTests()
    {
        IdentityMappingConfig.RegisterMappings();
    }

    public AccountServiceTests()
    {
        _userManagerMock = MockUserManager();
        _signInManagerMock = MockSignInManager();
        _emailServiceMock = new Mock<IEmailService>();
        _fileServiceMock = new Mock<IFileService>();
        _profileServiceMock = new Mock<IProfileService>();

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c[It.IsAny<string>()]).Returns("test");

        _sut = new AccountService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _emailServiceMock.Object,
            _fileServiceMock.Object,
            _profileServiceMock.Object
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

        result.UserName.Should().Be("testuser");
        result.IsVerified.Should().BeTrue();
        result.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ThrowsDomainValidationException()
    {
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync((AppUser?)null);

        Func<Task> act = () => _sut.LoginAsync(new LoginRequest("testuser", "wrong"), false);

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task LoginAsync_LockedOut_ThrowsDomainValidationException()
    {
        var user = CreateActiveUser();
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync(user);
        _signInManagerMock
            .Setup(m => m.PasswordSignInAsync("testuser", "Pass123!", false, true))
            .ReturnsAsync(SignInResult.LockedOut);

        Func<Task> act = () => _sut.LoginAsync(new LoginRequest("testuser", "Pass123!"), false);

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ThrowsDomainValidationException()
    {
        var user = CreateInactiveUser();
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync(user);

        Func<Task> act = () => _sut.LoginAsync(new LoginRequest("testuser", "Pass123!"), false);

        await act.Should().ThrowAsync<DomainValidationException>();
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
    public async Task RegisterAsync_DuplicateUsername_ThrowsDomainValidationException()
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

        Func<Task> act = () => _sut.RegisterAsync(request, "http://localhost");

        await act.Should().ThrowAsync<DomainValidationException>();
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
            .ReturnsAsync(IdentityResult.Success)
            .Callback<AppUser, string>((u, _) => u.EmailConfirmed = true);

        var result = await _sut.ConfirmAccountAsync("user1", encodedToken);

        result.IsVerified.Should().BeTrue();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAccountAsync_InvalidToken_ThrowsDomainValidationException()
    {
        var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(
            System.Text.Encoding.UTF8.GetBytes("some-token")
        );
        _userManagerMock.Setup(m => m.FindByIdAsync("user1")).ReturnsAsync((AppUser?)null);

        Func<Task> act = () => _sut.ConfirmAccountAsync("user1", encodedToken);

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    // ==================== FORGOT/RESET PASSWORD TESTS ====================

    [Fact]
    public async Task ForgotPasswordAsync_Always_ReturnsGenericResponse()
    {
        _userManagerMock.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync((AppUser?)null);

        var result = await _sut.ForgotPasswordAsync(
            new ForgotPasswordRequest("testuser", "http://localhost")
        );

        result.IsVerified.Should().BeFalse();
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

        result.IsVerified.Should().BeFalse();
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

        result.IsVerified.Should().BeTrue();
        _userManagerMock.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
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
