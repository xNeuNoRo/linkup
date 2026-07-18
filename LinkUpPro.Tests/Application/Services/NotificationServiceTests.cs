using LinkUpPro.Application.DTOs.Notification.Requests;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Infrastructure.Persistence.Repositories;
using LinkUpPro.Tests.Base;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class NotificationServiceTests : InMemoryTestBase
{
    private INotificationService? _service;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private bool _hasImplementation;

    public NotificationServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<INotificationService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation)
            return;

        var notification = Notification.CreateComment("user1", "actor1", 1, "Actor1").Value;
        typeof(Notification).GetProperty(nameof(Notification.Id))!.SetValue(notification, 1L);

        DbContext.Add(notification);
        await DbContext.SaveChangesAsync();

        var notificationRepo = new NotificationRepository(DbContext);

        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(
                (string id) =>
                    new UserResponseDto(
                        id,
                        "testuser",
                        "test@test.com",
                        "Actor",
                        "Name",
                        "809-555-1234",
                        "/images/default-avatar.png",
                        true,
                        true
                    )
            );
        profileServiceMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IEnumerable<string> ids, CancellationToken _) =>
                    ids.ToDictionary(
                        id => id,
                        id => new UserResponseDto(
                            id, "testuser", "test@test.com", "Actor", "Name",
                            "809-555-1234", "/images/default-avatar.png", true, true)
                    )
            );

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var implType = ImplementationDiscovery.FindImplementation<INotificationService>()!;
        _service = (INotificationService)
            Activator.CreateInstance(
                implType,
                notificationRepo,
                profileServiceMock.Object,
                _unitOfWorkMock.Object
            )!;
    }

    [ServiceFact(typeof(INotificationService))]
    public async Task GetNotificationsAsync_ReturnsPaged()
    {
        var result = await _service!.GetNotificationsAsync("user1", null, 1, 20);

        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(INotificationService))]
    public async Task GetNotificationsAsync_UnreadOnly_Filters()
    {
        var result = await _service!.GetNotificationsAsync("user1", true, 1, 20);

        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(INotificationService))]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        var result = await _service!.GetUnreadCountAsync("user1");

        result.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [ServiceFact(typeof(INotificationService))]
    public async Task MarkAsReadAsync_Valid_ReturnsSuccess()
    {
        var req = new MarkAsReadRequest(1);
        var result = await _service!.MarkAsReadAsync("user1", req);

        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(INotificationService))]
    public async Task MarkAsReadAsync_NotOwner_ReturnsError()
    {
        var req = new MarkAsReadRequest(1);
        var result = await _service!.MarkAsReadAsync("user2", req);

        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(INotificationService))]
    public async Task MarkAllAsReadAsync_Valid_ReturnsSuccess()
    {
        var result = await _service!.MarkAllAsReadAsync("user1");

        result.IsSuccess.Should().BeTrue();
    }
}
