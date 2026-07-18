using FluentAssertions;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LinkUpPro.Tests.Application.ViewModels.Shared.ValidationAttributes;

public class RequiredFileAttributeTests
{
    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsFalse()
    {
        // Arrange
        var attr = new RequiredFileAttribute();

        // Act
        var result = attr.IsValid(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenValueIsNotIFormFile_ReturnsFalse()
    {
        // Arrange
        var attr = new RequiredFileAttribute();

        // Act
        var result = attr.IsValid("not a file");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenIFormFileIsEmpty_ReturnsFalse()
    {
        // Arrange
        var attr = new RequiredFileAttribute();
        var file = new FormFile(Stream.Null, 0, 0, "name", "filename.txt");

        // Act
        var result = attr.IsValid(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenIFormFileIsValid_ReturnsTrue()
    {
        // Arrange
        var attr = new RequiredFileAttribute();
        var ms = new MemoryStream([1, 2, 3]);
        var file = new FormFile(ms, 0, ms.Length, "name", "filename.png");

        // Act
        var result = attr.IsValid(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_IsRequiredAttribute_True()
    {
        // Arrange
        var attr = new RequiredFileAttribute();

        // Assert
        attr.Should().BeAssignableTo<System.ComponentModel.DataAnnotations.RequiredAttribute>();
    }
}
