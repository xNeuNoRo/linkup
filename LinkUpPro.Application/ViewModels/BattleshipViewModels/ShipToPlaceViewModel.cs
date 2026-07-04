namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

public class ShipToPlaceViewModel
{
    public int Size { get; set; }
    public string Label { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Index { get; set; }
    public bool IsPlaced { get; set; }

    private static readonly Dictionary<int, (string Name, string Icon)> ShipInfo = new()
    {
        { 5, ("Portaaviones", "anchor") },
        { 4, ("Acorazado", "ship") },
        { 3, ("Submarino", "waves") },
        { 2, ("Destructor", "zap") },
    };

    private static readonly Dictionary<int, string[]> ShipNamesByIndex = new()
    {
        { 3, ["Submarino", "Crucero"] },
    };

    public static string GetName(int size, int index = 1) =>
        ShipNamesByIndex.TryGetValue(size, out var names) && index <= names.Length
            ? names[index - 1]
            : ShipInfo.GetValueOrDefault(size).Name ?? $"Barco de {size}";

    public static string GetIcon(int size) =>
        ShipInfo.GetValueOrDefault(size).Icon ?? "anchor";
}
