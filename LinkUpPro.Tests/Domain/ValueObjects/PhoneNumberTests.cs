using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("809-555-1234")]
    [InlineData("829-555-1234")]
    [InlineData("849-555-1234")]
    public void Create_ValidDominicanPhoneNumber_ReturnsPhoneNumber(string value)
    {
        // Arrange & Act
        var phoneNumber = PhoneNumber.Create(value);

        // Assert
        Assert.Equal(value, phoneNumber.Value);
        Assert.Equal(value.Replace("-", string.Empty), phoneNumber.DigitsOnly);
        Assert.Equal(value, phoneNumber.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("8095551234")]
    [InlineData("809-55-1234")]
    [InlineData("809-555-123")]
    [InlineData("801-555-1234")]
    [InlineData("809-ABC-1234")]
    public void Create_InvalidDominicanPhoneNumber_ThrowsDomainException(string value)
    {
        // Arrange & Act
        void Act() => _ = PhoneNumber.Create(value);

        // Assert
        Assert.Throws<DomainException>(Act);
    }
}
