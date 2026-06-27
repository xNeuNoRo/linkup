using FluentValidation.TestHelper;
using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Application.Validators.Battleship;

namespace LinkUpPro.Tests.Application.Validators;

public class AttackRequestValidatorTests
{
    private readonly AttackRequestValidator _validator = new();

    [Fact]
    public void AttackRequest_XNegative_ReturnsError()
    {
        var request = new AttackRequest(X: -1, Y: 5);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.X);
    }

    [Fact]
    public void AttackRequest_XTooLarge_ReturnsError()
    {
        var request = new AttackRequest(X: 12, Y: 5);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.X);
    }

    [Fact]
    public void AttackRequest_YNegative_ReturnsError()
    {
        var request = new AttackRequest(X: 5, Y: -1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Y);
    }

    [Fact]
    public void AttackRequest_YTooLarge_ReturnsError()
    {
        var request = new AttackRequest(X: 5, Y: 12);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Y);
    }

    [Fact]
    public void AttackRequest_ValidCoordinates_Passes()
    {
        var request = new AttackRequest(X: 5, Y: 5);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AttackRequest_BoundaryCoordinates_Passes()
    {
        var corners = new[]
        {
            new AttackRequest(0, 0),
            new AttackRequest(11, 0),
            new AttackRequest(0, 11),
            new AttackRequest(11, 11),
        };

        foreach (var request in corners)
        {
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
