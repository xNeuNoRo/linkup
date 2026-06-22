using LinkUpPro.Application.DTOs.Comment.Requests;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Tests.Base;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class CommentServiceTests : InMemoryTestBase
{
    private ICommentService? _service;
    private bool _hasImplementation;

    public CommentServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<ICommentService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation) return;

        var implType = ImplementationDiscovery.FindImplementation<ICommentService>()!;
        _service = (ICommentService)Activator.CreateInstance(implType,
            null!, null!, null!)!;
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task CreateAsync_ValidContent_ReturnsComment()
    {
        var request = new CreateCommentRequest(1, "Great post!");
        var result = await _service!.CreateAsync("user1", request);
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Great post!");
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task CreateAsync_EmptyContent_ReturnsError()
    {
        var request = new CreateCommentRequest(1, "");
        var result = await _service!.CreateAsync("user1", request);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task CreateAsync_ContentTooLong_ReturnsError()
    {
        var request = new CreateCommentRequest(1, new string('x', 501));
        var result = await _service!.CreateAsync("user1", request);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task CreateReplyAsync_Valid_ReturnsReply()
    {
        var request = new CreateReplyRequest(1, "Nice reply");
        var result = await _service!.CreateReplyAsync("user1", request);
        result.IsSuccess.Should().BeTrue();
        result.Value.ParentCommentId.Should().Be(1);
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task UpdateAsync_NotAuthor_ReturnsError()
    {
        var request = new CreateCommentRequest(1, "Original");
        var created = await _service!.CreateAsync("user1", request);
        if (!created.IsSuccess) return;

        var updateReq = new UpdateCommentRequest("Edited");
        var result = await _service!.UpdateAsync("user2", created.Value.Id, updateReq);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task DeleteAsync_NotAuthor_ReturnsError()
    {
        var result = await _service!.DeleteAsync("user2", 1);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task GetPostCommentsAsync_ReturnsList()
    {
        var result = await _service!.GetPostCommentsAsync("user1", 1);
        result.Should().NotBeNull();
    }
}
