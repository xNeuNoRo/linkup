using FluentValidation.TestHelper;
using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Application.Validators.Battleship;

namespace LinkUpPro.Tests.Application.Validators;

public class PlaceShipRequestValidatorTests
{
    private readonly PlaceShipRequestValidator _validator = new();

    [Fact]
    public void PlaceShipRequest_StartXNegative_ReturnsError()
    {
        var request = new PlaceShipRequest(ShipSize: 2, StartX: -1, StartY: 5, Direction: 1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StartX);
    }

    [Fact]
    public void PlaceShipRequest_StartXTooLarge_ReturnsError()
    {
        var request = new PlaceShipRequest(ShipSize: 2, StartX: 12, StartY: 5, Direction: 1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StartX);
    }

    [Fact]
    public void PlaceShipRequest_StartYNegative_ReturnsError()
    {
        var request = new PlaceShipRequest(ShipSize: 2, StartX: 5, StartY: -1, Direction: 1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StartY);
    }

    [Fact]
    public void PlaceShipRequest_StartYTooLarge_ReturnsError()
    {
        var request = new PlaceShipRequest(ShipSize: 2, StartX: 5, StartY: 12, Direction: 1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StartY);
    }

    [Fact]
    public void PlaceShipRequest_InvalidDirection_ReturnsError()
    {
        var request = new PlaceShipRequest(ShipSize: 2, StartX: 5, StartY: 5, Direction: 0);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Direction);
    }

    [Fact]
    public void PlaceShipRequest_DirectionOutOfRange_ReturnsError()
    {
        var request = new PlaceShipRequest(ShipSize: 2, StartX: 5, StartY: 5, Direction: 5);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Direction);
    }

    [Fact]
    public void PlaceShipRequest_InvalidShipSize_ReturnsError()
    {
        var request = new PlaceShipRequest(ShipSize: 1, StartX: 5, StartY: 5, Direction: 1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ShipSize);
    }

    [Fact]
    public void PlaceShipRequest_ShipSize6_ReturnsError()
    {
        var request = new PlaceShipRequest(ShipSize: 6, StartX: 5, StartY: 5, Direction: 1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ShipSize);
    }

    [Fact]
    public void PlaceShipRequest_ValidSize2_Passes()
    {
        var request = new PlaceShipRequest(ShipSize: 2, StartX: 5, StartY: 5, Direction: 1);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PlaceShipRequest_ValidSize5_Passes()
    {
        var request = new PlaceShipRequest(ShipSize: 5, StartX: 0, StartY: 0, Direction: 2);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PlaceShipRequest_ValidAllDirections_Pass()
    {
        for (int dir = 1; dir <= 4; dir++)
        {
            var request = new PlaceShipRequest(ShipSize: 3, StartX: 5, StartY: 5, Direction: dir);
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    [Fact]
    public void PlaceShipRequest_BoundaryCoordinates_Pass()
    {
        // Test all 4 corners of the board
        var corners = new[]
        {
            new PlaceShipRequest(2, 0, 0, 1),
            new PlaceShipRequest(2, 11, 0, 1),
            new PlaceShipRequest(2, 0, 11, 1),
            new PlaceShipRequest(2, 11, 11, 1),
        };

        foreach (var request in corners)
        {
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
