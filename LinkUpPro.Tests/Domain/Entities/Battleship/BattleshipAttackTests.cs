using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.Entities.Battleship;

public class BattleshipAttackTests
{
    [Fact]
    public void Record_ValidData_CreatesAttack()
    {
        // Arrange
        var target = Coordinates.Create(5, 7);

        // Act
        var result = BattleshipAttack.Record(1, "attacker", target, isHit: true, targetShipId: 10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsHit);
        Assert.Equal(10, result.Value.TargetShipId);
        Assert.Equal(target, result.Value.GetTarget());
    }
}
