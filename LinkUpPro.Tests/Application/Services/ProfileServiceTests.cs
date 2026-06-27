using LinkUpPro.Application.DTOs.Profile.Requests;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Infrastructure.Identity.Entities;
using LinkUpPro.Infrastructure.Identity.Services;
using LinkUpPro.Tests.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class ProfileServiceTests : InMemoryTestBase
{
    private IProfileService? _service;
    private Mock<UserManager<AppUser>> _userManagerMock = null!;
    private Mock<IFileService> _fileServiceMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    public ProfileServiceTests() { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _fileServiceMock = new Mock<IFileService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _service = new ProfileService(
            CreateUserManager().Object,
            CreateSignInManager().Object,
            _fileServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task GetProfileAsync_ExistingUser_ReturnsProfile()
    {
        var user = CreateUser("user1", "john", "John", "Doe", true, true);
        SetupFindById("user1", user);

        var result = await _service!.GetProfileAsync("user1");

        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().Be("John");
        result.Value.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task GetProfileAsync_NonexistentUser_ReturnsFailure()
    {
        var result = await _service!.GetProfileAsync("nobody");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "User.NotFound");
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidData_UpdatesAndPersists()
    {
        var user = CreateUser("user1", "john", "John", "Doe", true, true, "809-555-1234");
        SetupFindById("user1", user);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var request = new UpdateProfileRequest("Jane", "Smith", "829-555-5678", null);
        var result = await _service!.UpdateProfileAsync("user1", request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().Be("Jane");
        user.FirstName.Should().Be("Jane");
        user.PhoneNumber.Should().Be("829-555-5678");
    }

    [Fact]
    public async Task UpdateProfileAsync_InvalidPhone_ReturnsFailure()
    {
        var user = CreateUser("user1", "john", "John", "Doe", true, true);
        SetupFindById("user1", user);

        var request = new UpdateProfileRequest("Jane", "Smith", "123-4567", null);

        var result = await _service!.UpdateProfileAsync("user1", request);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "User.InvalidPhoneNumber");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNewPhoto_DeletesOldPhoto()
    {
        var user = CreateUser(
            "user1",
            "john",
            "John",
            "Doe",
            true,
            true,
            "809-555-1234",
            "/uploads/old-photo.jpg"
        );
        SetupFindById("user1", user);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var formFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        formFile.Setup(x => x.Length).Returns(1024);
        _fileServiceMock.Setup(x => x.IsImageValid(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>())).Returns(true);
        _fileServiceMock
            .Setup(x => x.UploadFileAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("/uploads/new-photo.jpg");
        _fileServiceMock.Setup(x => x.DeleteFile("/uploads/old-photo.jpg"));

        var request = new UpdateProfileRequest(
            "John",
            "Doe",
            "809-555-1234",
            formFile.Object
        );
        var result = await _service!.UpdateProfileAsync("user1", request);

        result.IsSuccess.Should().BeTrue();
        _fileServiceMock.Verify(x => x.DeleteFile("/uploads/old-photo.jpg"), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_CorrectCurrentStrong_ReturnsRequiresReLogin()
    {
        var user = CreateUser("user1", "john", "John", "Doe", true, true);
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, "OldPass1!");
        SetupFindById("user1", user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "OldPass1!")).ReturnsAsync(true);
        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(user, "OldPass1!", "NewPass2@"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var request = new ChangePasswordRequest("OldPass1!", "NewPass2@", "NewPass2@");
        var result = await _service!.ChangePasswordAsync("user1", request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RequiresReLogin.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrent_ReturnsFailure()
    {
        var user = CreateUser("user1", "john", "John", "Doe", true, true);
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, "RealPass1!");
        SetupFindById("user1", user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "Wrong1!")).ReturnsAsync(false);

        var request = new ChangePasswordRequest("Wrong1!", "NewPass2@", "NewPass2@");

        var result = await _service!.ChangePasswordAsync("user1", request);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Password.Invalid");
    }

    [Fact]
    public async Task ChangePasswordAsync_NewEqualsCurrent_ReturnsFailure()
    {
        var user = CreateUser("user1", "john", "John", "Doe", true, true);
        SetupFindById("user1", user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "SamePass1!")).ReturnsAsync(true);

        var request = new ChangePasswordRequest("SamePass1!", "SamePass1!", "SamePass1!");

        var result = await _service!.ChangePasswordAsync("user1", request);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Password.SameAsCurrent");
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ReturnsDto()
    {
        var user = CreateUser(
            "user1",
            "john",
            "John",
            "Doe",
            true,
            true,
            "809-555-1234",
            null,
            "john@test.com"
        );
        _userManagerMock.Setup(x => x.FindByEmailAsync("john@test.com")).ReturnsAsync(user);

        var result = await _service!.GetByEmailAsync("john@test.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("john@test.com");
    }

    [Fact]
    public async Task GetByEmailAsync_Nonexistent_ReturnsNull()
    {
        var result = await _service!.GetByEmailAsync("nonexistent@test.com");

        result.Should().BeNull();
    }

    private void SetupFindById(string id, AppUser user)
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(id)).ReturnsAsync(user);
    }

    private Mock<UserManager<AppUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
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
        _userManagerMock.CallBase = false;
        return _userManagerMock;
    }

    private Mock<SignInManager<AppUser>> CreateSignInManager()
    {
        var ctx = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
        var mock = new Mock<SignInManager<AppUser>>(
            _userManagerMock.Object,
            ctx.Object,
            claimsFactory.Object,
            null!,
            null!,
            null!,
            null!
        );
        mock.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    private static AppUser CreateUser(
        string id,
        string userName,
        string firstName,
        string lastName,
        bool isActive,
        bool emailConfirmed,
        string? phone = "809-555-1234",
        string? photoPath = null,
        string? email = null
    )
    {
        return new AppUser
        {
            Id = id,
            UserName = userName,
            Email = email ?? $"{userName}@test.com",
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phone,
            ProfilePicturePath = photoPath ?? "/images/default-avatar.png",
            IsActive = isActive,
            EmailConfirmed = emailConfirmed,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
