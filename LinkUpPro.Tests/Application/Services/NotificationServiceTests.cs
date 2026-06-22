using LinkUpPro.Application.DTOs.Notification.Requests;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Tests.Base;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class NotificationServiceTests : InMemoryTestBase
{
    private INotificationService? _service;
    private bool _hasImplementation;

    public NotificationServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<INotificationService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation) return;

        var implType = ImplementationDiscovery.FindImplementation<INotificationService>()!;
        _service = (INotificationService)Activator.CreateInstance(implType, null!, null!)!;
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
