using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LinkUpPro.Tests.Application.ViewModels.Shared.ValidationAttributes;

public class RequiredIfAttributeTests
{
    private class TestViewModel
    {
        public int ContentType { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? YouTubeUrl { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    [Fact]
    public void IsValid_WhenDependentPropertyMatchesTarget_NoValue_ReturnsError()
    {
        // Arrange
        var attr = new RequiredIfAttribute(nameof(TestViewModel.ContentType), 1);
        var instance = new TestViewModel { ContentType = 1 };
        var ctx = new ValidationContext(instance) { MemberName = nameof(TestViewModel.ImageFile) };

        // Act
        var result = attr.GetValidationResult(null, ctx);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenDependentPropertyDoesNotMatchTarget_NoValue_ReturnsSuccess()
    {
        // Arrange
        var attr = new RequiredIfAttribute(nameof(TestViewModel.ContentType), 1);
        var instance = new TestViewModel { ContentType = 2 };
        var ctx = new ValidationContext(instance) { MemberName = nameof(TestViewModel.ImageFile) };

        // Act
        var result = attr.GetValidationResult(null, ctx);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenDependentPropertyMatchesTarget_WithValue_ReturnsSuccess()
    {
        // Arrange
        var attr = new RequiredIfAttribute(nameof(TestViewModel.ContentType), 1);
        var instance = new TestViewModel { ContentType = 1 };
        var ctx = new ValidationContext(instance) { MemberName = nameof(TestViewModel.ImageFile) };

        // Act
        var result = attr.GetValidationResult("valor", ctx);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WithInverted_DoesNotRequire_WhenTargetMatches()
    {
        // Arrange
        var attr = new RequiredIfAttribute(nameof(TestViewModel.ContentType), 1, inverted: true);
        var instance = new TestViewModel { ContentType = 1 };
        var ctx = new ValidationContext(instance) { MemberName = nameof(TestViewModel.ImageFile) };

        // Act
        var result = attr.GetValidationResult(null, ctx);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WithInverted_Requires_WhenTargetDoesNotMatch()
    {
        // Arrange
        var attr = new RequiredIfAttribute(nameof(TestViewModel.ContentType), 1, inverted: true);
        var instance = new TestViewModel { ContentType = 2 };
        var ctx = new ValidationContext(instance) { MemberName = nameof(TestViewModel.ImageFile) };

        // Act
        var result = attr.GetValidationResult(null, ctx);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenStringValueIsEmpty_NoValueConsideredPresent_ReturnsError()
    {
        // Arrange
        var attr = new RequiredIfAttribute(nameof(TestViewModel.ContentType), 1);
        var instance = new TestViewModel { ContentType = 1 };
        var ctx = new ValidationContext(instance) { MemberName = nameof(TestViewModel.YouTubeUrl) };

        // Act
        var result = attr.GetValidationResult("   ", ctx);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenStringValueIsNonEmpty_ReturnsSuccess()
    {
        // Arrange
        var attr = new RequiredIfAttribute(nameof(TestViewModel.ContentType), 2);
        var instance = new TestViewModel { ContentType = 2 };
        var ctx = new ValidationContext(instance) { MemberName = nameof(TestViewModel.YouTubeUrl) };

        // Act
        var result = attr.GetValidationResult("https://youtube.com/watch?v=abc12345678", ctx);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenDependentPropertyDoesNotExist_ReturnsError()
    {
        // Arrange
        var attr = new RequiredIfAttribute("NonExistentProperty", 1);
        var instance = new TestViewModel();
        var ctx = new ValidationContext(instance) { MemberName = nameof(TestViewModel.ImageFile) };

        // Act
        var result = attr.GetValidationResult(null, ctx);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
    }

    [Fact]
    public void FormatErrorMessage_IncludesPropertyName()
    {
        // Arrange
        var attr = new RequiredIfAttribute(nameof(TestViewModel.ContentType), 1);

        // Act
        var message = attr.FormatErrorMessage("ImageFile");

        // Assert
        message.Should().Contain("ImageFile");
        message.Should().Contain(nameof(TestViewModel.ContentType));
    }
}
