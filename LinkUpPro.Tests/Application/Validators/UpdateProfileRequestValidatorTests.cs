using FluentValidation.TestHelper;
using LinkUpPro.Application.DTOs.Profile.Requests;
using LinkUpPro.Application.Validators.Profile;

namespace LinkUpPro.Tests.Application.Validators;

public class UpdateProfileRequestValidatorTests
{
    private readonly UpdateProfileRequestValidator _validator = new();

    [Fact]
    public void UpdateProfileRequest_EmptyFirstName_ReturnsError()
    {
        var request = new UpdateProfileRequest("", "Doe", "809-555-1234");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void UpdateProfileRequest_WhitespaceFirstName_ReturnsError()
    {
        var request = new UpdateProfileRequest("   ", "Doe", "809-555-1234");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void UpdateProfileRequest_EmptyLastName_ReturnsError()
    {
        var request = new UpdateProfileRequest("John", "", "809-555-1234");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void UpdateProfileRequest_WhitespaceLastName_ReturnsError()
    {
        var request = new UpdateProfileRequest("John", "   ", "809-555-1234");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void UpdateProfileRequest_InvalidPhoneFormat_ReturnsError()
    {
        var request = new UpdateProfileRequest("John", "Doe", "123-4567");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void UpdateProfileRequest_EmptyPhone_ReturnsError()
    {
        var request = new UpdateProfileRequest("John", "Doe", "");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void UpdateProfileRequest_ValidPhoneFormat809_Passes()
    {
        var request = new UpdateProfileRequest("John", "Doe", "809-555-1234");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProfileRequest_ValidPhoneFormat829_Passes()
    {
        var request = new UpdateProfileRequest("John", "Doe", "829-555-1234");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProfileRequest_ValidPhoneFormat849_Passes()
    {
        var request = new UpdateProfileRequest("John", "Doe", "849-555-1234");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProfileRequest_AllValid_Passes()
    {
        var request = new UpdateProfileRequest("John", "Doe", "809-555-1234");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
