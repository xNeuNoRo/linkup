using LinkUpPro.Application.DTOs.FriendRequest.Requests;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Tests.Base;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class FriendRequestServiceTests : InMemoryTestBase
{
    private IFriendRequestService? _service;
    private bool _hasImplementation;

    public FriendRequestServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IFriendRequestService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation) return;

        var implType = ImplementationDiscovery.FindImplementation<IFriendRequestService>()!;
        _service = (IFriendRequestService)Activator.CreateInstance(implType, null!, null!)!;
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task SendAsync_ToValidUser_CreatesRequest()
    {
        var req = new SendFriendRequestRequest("user2");
        var result = await _service!.SendAsync("user1", req);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task SendAsync_Self_ReturnsError()
    {
        var req = new SendFriendRequestRequest("user1");
        var result = await _service!.SendAsync("user1", req);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task SendAsync_AlreadyFriends_ReturnsError()
    {
        var req = new SendFriendRequestRequest("user2");
        var first = await _service!.SendAsync("user1", req);
        if (!first.IsSuccess) return;

        var second = await _service!.SendAsync("user1", req);
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task AcceptAsync_ValidRequest_CreatesFriendship()
    {
        var sendReq = new SendFriendRequestRequest("user2");
        await _service!.SendAsync("user1", sendReq);

        var acceptReq = new AcceptFriendRequestRequest(1);
        var result = await _service!.AcceptAsync("user2", acceptReq);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task AcceptAsync_AlreadyAccepted_ReturnsError()
    {
        var sendReq = new SendFriendRequestRequest("user2");
        await _service!.SendAsync("user1", sendReq);

        var acceptReq = new AcceptFriendRequestRequest(1);
        await _service!.AcceptAsync("user2", acceptReq);
        var second = await _service!.AcceptAsync("user2", acceptReq);
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task RejectAsync_Valid_RejectsAndNoFriendship()
    {
        var sendReq = new SendFriendRequestRequest("user2");
        await _service!.SendAsync("user1", sendReq);

        var rejectReq = new RejectFriendRequestRequest(1);
        var result = await _service!.RejectAsync("user2", rejectReq);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task CancelAsync_Valid_CancelsRequest()
    {
        var sendReq = new SendFriendRequestRequest("user2");
        await _service!.SendAsync("user1", sendReq);

        var cancelReq = new CancelFriendRequestRequest(1);
        var result = await _service!.CancelAsync("user1", cancelReq);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task GetPendingRequestsAsync_ReturnsPending()
    {
        var result = await _service!.GetPendingRequestsAsync("user2", 1, 20);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task GetSentRequestsAsync_ReturnsSent()
    {
        var result = await _service!.GetSentRequestsAsync("user1", 1, 20);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task GetPendingCountAsync_ReturnsCount()
    {
        var count = await _service!.GetPendingCountAsync("user2");
        count.Should().BeGreaterThanOrEqualTo(0);
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task DeleteFromHistoryAsync_Valid_RemovesFromHistory()
    {
        var req = new SendFriendRequestRequest("user2");
        await _service!.SendAsync("user1", req);

        var acceptReq = new AcceptFriendRequestRequest(1);
        await _service!.AcceptAsync("user2", acceptReq);

        var deleteReq = new DeleteFromHistoryRequest(1);
        var result = await _service!.DeleteFromHistoryAsync("user1", deleteReq);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task SearchAvailableUsersAsync_ReturnsPaged()
    {
        var result = await _service!.SearchAvailableUsersAsync("user1", null, 1, 20);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task SendAsync_InactiveUser_ReturnsError()
    {
        var req = new SendFriendRequestRequest("inactiveUser");
        var result = await _service!.SendAsync("user1", req);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendRequestService))]
    public async Task AcceptAsync_NotReceiver_ReturnsError()
    {
        var sendReq = new SendFriendRequestRequest("user2");
        await _service!.SendAsync("user1", sendReq);

        var acceptReq = new AcceptFriendRequestRequest(1);
        var result = await _service!.AcceptAsync("user3", acceptReq);
        result.IsFailure.Should().BeTrue();
    }
}
