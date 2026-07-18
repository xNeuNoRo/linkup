namespace LinkUpPro.Application.Common.Avatar;

public static class AvatarResolver
{
    public const string DefaultAvatarPath = "/images/default-avatar.png";

    private static readonly string[] Icons =
    [
        "user",
        "user-circle",
        "user-round",
        "user-check",
        "smile",
        "smile-plus",
        "heart",
        "star",
        "sun",
        "moon",
        "cloud",
        "flower",
        "sparkles",
        "zap",
        "compass",
        "crown",
        "feather",
        "gem",
        "rocket",
        "leaf",
    ];

    private static readonly (string from, string to)[] Palettes =
    [
        ("indigo-600", "violet-600"),
        ("blue-600", "cyan-500"),
        ("teal-500", "emerald-500"),
        ("emerald-500", "lime-500"),
        ("amber-500", "orange-500"),
        ("rose-500", "pink-500"),
        ("purple-600", "fuchsia-500"),
        ("orange-500", "red-500"),
    ];

    public static bool IsDefaultAvatar(string? path) =>
        string.IsNullOrWhiteSpace(path) || path == DefaultAvatarPath;

    public static string GetIcon(string name) => Icons[Math.Abs(name.GetHashCode()) % Icons.Length];

    public static (string from, string to) GetPalette(string name) =>
        Palettes[Math.Abs(name.GetHashCode()) % Palettes.Length];

    public static string GetInitials(string name)
    {
        var initials = string.IsNullOrEmpty(name)
            ? "U"
            : string.Join("", name.Split(' ').Where(w => w.Length > 0).Select(w => w[0])).ToUpper();
        return initials.Length > 2 ? initials[..2] : initials;
    }

    public static string GetIconClass(string size)
    {
        return size switch
        {
            "xs" => "w-3 h-3",
            "sm" => "w-4 h-4",
            "md" => "w-5 h-5",
            "lg" => "w-8 h-8",
            "xl" => "w-10 h-10",
            _ => "w-5 h-5",
        };
    }

    public static string GetSizeClass(string? size)
    {
        return (size ?? "md") switch
        {
            "xs" => "w-6 h-6",
            "sm" => "w-8 h-8",
            "md" => "w-10 h-10",
            "lg" => "w-16 h-16",
            "xl" => "w-24 h-24",
            _ => "w-10 h-10",
        };
    }
}
