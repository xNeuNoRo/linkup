using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Infrastructure.Identity.Seeds;

public static class DefaultRoles
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static async Task SeedAsync(RoleManager<AppRole> roleManager)
    {
        foreach (var role in new[] { Admin, User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new AppRole(role));
            }
        }
    }
}
