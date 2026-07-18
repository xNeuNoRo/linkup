using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_ValidEmail_NormalizesValue()
    {
        // Arrange & Act
        var email = Email.Create("  USER@Example.COM ");

        // Assert
        Assert.Equal("user@example.com", email.Value);
        Assert.Equal("user@example.com", email.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("a@b")]
    public void Create_InvalidEmail_ThrowsDomainException(string value)
    {
        // Arrange & Act
        void Act() => _ = Email.Create(value);

        // Assert
        Assert.Throws<DomainException>(Act);
    }
}
