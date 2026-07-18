using LinkUpPro.Domain.Common;

namespace LinkUpPro.Domain.Settings;

/// <summary>
/// Configuración relacionada con las reglas y parámetros del juego de Battleship
/// </summary>
public sealed class GameSettings
{
    public const string SectionName = "GameSettings";

    public int BoardSize { get; init; } = DomainConstants.BoardSize;

    public int TurnTimeoutHours { get; init; } =
        (int)DomainConstants.BattleshipTurnTimeout.TotalHours;

    public IReadOnlyList<int> RequiredFleetSizes { get; init; } =
        DomainConstants.RequiredBattleshipFleetSizes;

    public TimeSpan TurnTimeout => TimeSpan.FromHours(TurnTimeoutHours);

    public bool IsValidCoordinate(int x, int y) =>
        x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;

    public bool HasValidFleetComposition(IEnumerable<int> fleetSizes)
    {
        ArgumentNullException.ThrowIfNull(fleetSizes);

        return fleetSizes.Order().SequenceEqual(RequiredFleetSizes.Order());
    }
}
