using FluentValidation.TestHelper;
using LinkUpPro.Application.DTOs.Post.Requests;
using LinkUpPro.Application.Validators.Post;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LinkUpPro.Tests.Application.Validators;

public class CreatePostRequestValidatorTests
{
    private readonly CreatePostRequestValidator _validator = new();

    [Fact]
    public void CreatePostRequest_EmptyContent_ReturnsError()
    {
        var request = new CreatePostRequest("", 2, null, "https://youtu.be/abc", 1, true);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreatePostRequest_ContentTooLong_ReturnsError()
    {
        var longContent = new string('a', 1001);
        var request = new CreatePostRequest(longContent, 2, null, "https://youtu.be/abc", 1, true);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreatePostRequest_InvalidContentType_ReturnsError()
    {
        var request = new CreatePostRequest("Test", 0, null, "https://youtu.be/abc", 1, true);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact]
    public void CreatePostRequest_InvalidPrivacy_ReturnsError()
    {
        var request = new CreatePostRequest("Test", 2, null, "https://youtu.be/abc", 0, true);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Privacy);
    }

    [Fact]
    public void CreatePostRequest_ImageTypeWithoutFile_ReturnsError()
    {
        var request = new CreatePostRequest("Test", 1, null, null, 1, true);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ImageFile);
    }

    [Fact]
    public void CreatePostRequest_YouTubeTypeWithoutUrl_ReturnsError()
    {
        var request = new CreatePostRequest("Test", 2, null, "", 1, true);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.YouTubeUrl);
    }

    [Fact]
    public void CreatePostRequest_ValidYouTubePost_Passes()
    {
        var request = new CreatePostRequest(
            "Test content",
            2,
            null,
            "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            1,
            true
        );
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreatePostRequest_ValidImagePost_Passes()
    {
        var formFile = new Mock<IFormFile>();
        formFile.Setup(x => x.Length).Returns(1024);
        var request = new CreatePostRequest("Test content", 1, formFile.Object, null, 1, true);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
