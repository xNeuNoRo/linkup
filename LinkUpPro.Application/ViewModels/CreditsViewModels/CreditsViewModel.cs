namespace LinkUpPro.Application.ViewModels.CreditsViewModels;

public class CreditsViewModel
{
    public string ProjectName { get; set; } = "LinkUp Pro";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "Red social con motor de mini-juegos Battleship";
    public List<TeamMember> Team { get; set; } =
    [
        new TeamMember
        {
            Name = "Leonardo Enrique Tavarez Beltran",
            Initials = "LT",
            Role = "Project Master / Esclavizador Master",
            AvatarColor = "bg-amber-500",
            AvatarShadow = "shadow-amber-200 dark:shadow-amber-900/50",
            RoleColor = "text-amber-600 dark:text-amber-400",
            HoverColor = "hover:bg-amber-50 dark:hover:bg-amber-900/20",
            IconColor = "group-hover:text-amber-600",
            IsMaster = true,
        },
        new TeamMember
        {
            Name = "Angel Gonzalez Muñoz",
            Initials = "AM",
            Role = "Lead Developer",
            AvatarColor = "bg-indigo-600",
            AvatarShadow = "shadow-indigo-200 dark:shadow-indigo-900/50",
            RoleColor = "text-indigo-600 dark:text-indigo-400",
            HoverColor = "hover:bg-indigo-50 dark:hover:bg-indigo-900/20",
            IconColor = "group-hover:text-indigo-600",
            GitHubUrl = "https://github.com/xNeuNoRo",
        },
        new TeamMember
        {
            Name = "Isaias Jose Morillo Ferreras",
            Initials = "IF",
            Role = "Desarrollador de Software",
            AvatarColor = "bg-emerald-500",
            AvatarShadow = "shadow-emerald-200 dark:shadow-emerald-900/50",
            RoleColor = "text-emerald-600 dark:text-emerald-400",
            HoverColor = "hover:bg-emerald-50 dark:hover:bg-emerald-900/20",
            IconColor = "group-hover:text-emerald-600",
            GitHubUrl = "https://github.com/IsaiasMorillo",
        },
        new TeamMember
        {
            Name = "Engel Orlando Acosta Santos",
            Initials = "AS",
            Role = "Desarrollador de Software",
            AvatarColor = "bg-violet-500",
            AvatarShadow = "shadow-violet-200 dark:shadow-violet-900/50",
            RoleColor = "text-violet-600 dark:text-violet-400",
            HoverColor = "hover:bg-violet-50 dark:hover:bg-violet-900/20",
            IconColor = "group-hover:text-violet-600",
            GitHubUrl = "https://github.com/notengel",
        },
    ];
}

public class TeamMember
{
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = "bg-indigo-600";
    public string AvatarShadow { get; set; } = "shadow-indigo-200";
    public string RoleColor { get; set; } = "text-indigo-600";
    public string HoverColor { get; set; } = "hover:bg-indigo-50";
    public string IconColor { get; set; } = "group-hover:text-indigo-600";
    public string? GitHubUrl { get; set; }
    public bool IsMaster { get; set; }
}
