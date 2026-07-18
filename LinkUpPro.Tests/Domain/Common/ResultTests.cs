using LinkUpPro.Domain.Common;

namespace LinkUpPro.Tests.Domain.Common;

public class ResultTests
{
    [Fact]
    public void Success_NoValue_ReturnsSuccessfulResult()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_WithError_ReturnsFailedResult()
    {
        // Arrange
        var error = new DomainError("Post.ContentRequired", "Debe ingresar el contenido de la publicación.");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Success_WithValue_ExposesValue()
    {
        // Arrange & Act
        var result = Result<int>.Success(10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void Value_FailedResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Failure(new DomainError("Error", "Failure"));

        // Act
        void Act() => _ = result.Value;

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void Failure_WithoutErrors_ThrowsInvalidOperationException()
    {
        // Arrange & Act
        var act = () => Result.Failure([]);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }
}
