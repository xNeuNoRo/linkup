using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace LinkUpPro.Infrastructure.Identity.Seeds;

public static class DefaultAdminUser
{
    public static async Task SeedAsync(
        UserManager<AppUser> userManager,
        IConfiguration configuration
    )
    {
        var userName = configuration["SeedData:AdminUserName"] ?? "admin";
        if (await userManager.FindByNameAsync(userName) is not null)
            return;

        var user = new AppUser
        {
            UserName = userName,
            Email = configuration["SeedData:AdminEmail"] ?? "admin@linkuppro.com",
            EmailConfirmed = true,
            IsActive = true,
            PhoneNumber = configuration["SeedData:AdminPhone"] ?? "809-555-0001",
            PhoneNumberConfirmed = true,
            FirstName = configuration["SeedData:AdminFirstName"] ?? "Admin",
            LastName = configuration["SeedData:AdminLastName"] ?? "LinkUp",
            ProfilePicturePath =
                configuration["SeedData:AdminPicture"] ?? "/images/default-avatar.png",
        };

        var password = "123Pa$$word!";
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, DefaultRoles.Admin);
            await userManager.AddToRoleAsync(user, DefaultRoles.User);
        }
    }
}
