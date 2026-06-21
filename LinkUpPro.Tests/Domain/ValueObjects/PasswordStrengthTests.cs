using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.ValueObjects;

public class PasswordStrengthTests
{
    [Theory]
    [InlineData("abc", 1, PasswordStrengthLevel.Weak)]
    [InlineData("abcdefgh", 2, PasswordStrengthLevel.Weak)]
    [InlineData("Abcdefgh", 3, PasswordStrengthLevel.Medium)]
    [InlineData("Abcdefg1", 4, PasswordStrengthLevel.Medium)]
    [InlineData("Abcdefg1!", 5, PasswordStrengthLevel.Strong)]
    public void Calculate_ReturnsExpectedScoreAndLevel(
        string password,
        int expectedScore,
        PasswordStrengthLevel expectedLevel)
    {
        // Arrange & Act
        var strength = PasswordStrength.Calculate(password);

        // Assert
        Assert.Equal(expectedScore, strength.Score);
        Assert.Equal(expectedLevel, strength.Level);
        Assert.Equal(expectedLevel == PasswordStrengthLevel.Strong, strength.IsStrong);
    }

    [Fact]
    public void Calculate_StrongPassword_ReturnsAllCriteria()
    {
        // Arrange & Act
        var strength = PasswordStrength.Calculate("Abcdefg1!");

        // Assert
        Assert.True(strength.CriteriaMet.HasFlag(PasswordStrengthCriteria.MinimumLength));
        Assert.True(strength.CriteriaMet.HasFlag(PasswordStrengthCriteria.UppercaseLetter));
        Assert.True(strength.CriteriaMet.HasFlag(PasswordStrengthCriteria.LowercaseLetter));
        Assert.True(strength.CriteriaMet.HasFlag(PasswordStrengthCriteria.Digit));
        Assert.True(strength.CriteriaMet.HasFlag(PasswordStrengthCriteria.SpecialCharacter));
    }
}
