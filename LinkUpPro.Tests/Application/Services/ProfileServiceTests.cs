using LinkUpPro.Application.DTOs.Profile.Requests;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Infrastructure.Identity.Entities;
using LinkUpPro.Tests.Base;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class ProfileServiceTests : InMemoryTestBase
{
    private IProfileService? _service;
    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<IFileService> _fileServiceMock = null!;
    private UserManager<AppUser> _userManager = null!;
    private bool _hasImplementation;

    public ProfileServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IProfileService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        if (!_hasImplementation)
            return;

        _emailServiceMock = new Mock<IEmailService>();
        _fileServiceMock = new Mock<IFileService>();

        var store = new Mock<IUserStore<AppUser>>().Object;
        _userManager = new UserManager<AppUser>(
            store,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        var implType = ImplementationDiscovery.FindImplementation<IProfileService>()!;
        _service = (IProfileService)
            Activator.CreateInstance(
                implType,
                _userManager,
                null!, // SignInManager - dev will handle
                _emailServiceMock.Object,
                _fileServiceMock.Object,
                new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object
            )!;
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task GetProfileAsync_ExistingUser_ReturnsProfile()
    {
        var user = SeedUser("user1", "john", "John", "Doe", true, true);
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var result = await _service!.GetProfileAsync("user1");

        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.IsVerified.Should().BeTrue();
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task GetProfileAsync_NonexistentUser_ThrowsDomainValidationException()
    {
        Func<Task> act = () => _service!.GetProfileAsync("nobody");

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task UpdateProfileAsync_ValidData_UpdatesAndPersists()
    {
        var user = SeedUser("user1", "john", "John", "Doe", true, true, "809-555-1234");
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var request = new UpdateProfileRequest("Jane", "Smith", "829-555-5678", null);
        var result = await _service!.UpdateProfileAsync("user1", request);

        result.FirstName.Should().Be("Jane");
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task UpdateProfileAsync_InvalidPhone_ThrowsDomainValidationException()
    {
        var user = SeedUser("user1", "john", "John", "Doe", true, true);
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var request = new UpdateProfileRequest("Jane", "Smith", "123-4567", null);

        Func<Task> act = () => _service!.UpdateProfileAsync("user1", request);

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task UpdateProfileAsync_WithNewPhoto_DeletesOldPhoto()
    {
        var user = SeedUser(
            "user1",
            "john",
            "John",
            "Doe",
            true,
            true,
            "809-555-1234",
            "/uploads/old-photo.jpg"
        );
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        _fileServiceMock.Setup(x => x.DeleteFile("/uploads/old-photo.jpg"));

        var request = new UpdateProfileRequest(
            "John",
            "Doe",
            "809-555-1234",
            "/uploads/new-photo.jpg"
        );
        var result = await _service!.UpdateProfileAsync("user1", request);

        _fileServiceMock.Verify(x => x.DeleteFile("/uploads/old-photo.jpg"), Times.Once);
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task ChangePasswordAsync_CorrectCurrent_ReturnsRequiresReLogin()
    {
        var user = SeedUser("user1", "john", "John", "Doe", true, true);
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, "OldPass1!");
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var request = new ChangePasswordRequest("OldPass1!", "NewPass1!", "NewPass1!");
        var result = await _service!.ChangePasswordAsync("user1", request);

        result.RequiresReLogin.Should().BeTrue();
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task ChangePasswordAsync_WrongCurrent_ThrowsDomainValidationException()
    {
        var user = SeedUser("user1", "john", "John", "Doe", true, true);
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, "RealPass1!");
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var request = new ChangePasswordRequest("Wrong1!", "NewPass1!", "NewPass1!");

        Func<Task> act = () => _service!.ChangePasswordAsync("user1", request);

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task ChangePasswordAsync_NewEqualsCurrent_ThrowsDomainValidationException()
    {
        var user = SeedUser("user1", "john", "John", "Doe", true, true);
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, "SamePass1!");
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var request = new ChangePasswordRequest("SamePass1!", "SamePass1!", "SamePass1!");

        Func<Task> act = () => _service!.ChangePasswordAsync("user1", request);

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task UpdateProfileAsync_UnauthorizedUser_ThrowsDomainValidationException()
    {
        var user = SeedUser("user2", "john", "John", "Doe", true, true);
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var request = new UpdateProfileRequest("Jane", "Smith", "809-555-1234", null);

        Func<Task> act = () => _service!.UpdateProfileAsync("user2", request);

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task GetByEmailAsync_ExistingUser_ReturnsDto()
    {
        var user = SeedUser(
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
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var result = await _service!.GetByEmailAsync("john@test.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("john@test.com");
    }

    [ServiceFact(typeof(IProfileService))]
    public async Task GetByEmailAsync_Nonexistent_ReturnsNull()
    {
        var result = await _service!.GetByEmailAsync("nonexistent@test.com");

        result.Should().BeNull();
    }

    private static AppUser SeedUser(
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
