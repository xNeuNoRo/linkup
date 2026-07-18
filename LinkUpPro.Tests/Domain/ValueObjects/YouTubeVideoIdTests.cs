using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.ValueObjects;

public class YouTubeVideoIdTests
{
    [Theory]
    [InlineData("dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=test")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ")]
    public void Create_SupportedYouTubeValue_ExtractsVideoId(string value)
    {
        // Arrange & Act
        var videoId = YouTubeVideoId.Create(value);

        // Assert
        Assert.Equal("dQw4w9WgXcQ", videoId.Value);
        Assert.Equal("https://www.youtube.com/embed/dQw4w9WgXcQ", videoId.EmbedUrl);
        Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", videoId.WatchUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("https://example.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=short")]
    [InlineData("https://www.youtube.com/watch")]
    [InlineData("not-a-youtube-url")]
    public void Create_InvalidYouTubeValue_ThrowsDomainException(string value)
    {
        // Arrange & Act
        void Act() => _ = YouTubeVideoId.Create(value);

        // Assert
        Assert.Throws<DomainException>(Act);
    }
}
