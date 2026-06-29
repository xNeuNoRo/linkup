using LinkUpPro.Domain.Exceptions;

namespace LinkUpPro.Tests.Domain.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void DomainException_WithCode_ExposesCodeAndMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object?> { ["UserId"] = "abc" };

        // Act
        var exception = new DomainException("Rule violated.", "Rule.Code", metadata);

        // Assert
        Assert.Equal("Rule.Code", exception.Code);
        Assert.Equal("abc", exception.Metadata["UserId"]);
    }

    [Fact]
    public void DomainException_WithoutCode_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new DomainException("Rule violated.", " ");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void DomainValidationException_WithErrors_ExposesValidationErrors()
    {
        // Arrange
        var error = new DomainValidationError(
            "Content",
            "Debe ingresar el contenido de la publicación.",
            "Post.ContentRequired");

        // Act
        var exception = new DomainValidationException([error]);

        // Assert
        Assert.Equal("Domain.ValidationFailed", exception.Code);
        Assert.Single(exception.ValidationErrors);
        Assert.Contains("ValidationErrors", exception.Metadata.Keys);
    }
}
