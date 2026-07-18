using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace LinkUpPro.Infrastructure.Identity.Seeds;

public static class DefaultPlayerUser
{
    public static async Task SeedAsync(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        var userName = configuration["SeedData:Player1UserName"] ?? "player1";
        if (await userManager.FindByNameAsync(userName) is not null)
            return;

        var user = new AppUser
        {
            UserName = userName,
            Email = configuration["SeedData:Player1Email"] ?? "player1@linkuppro.com",
            EmailConfirmed = true,
            IsActive = true,
            PhoneNumber = configuration["SeedData:Player1Phone"] ?? "809-555-0002",
            PhoneNumberConfirmed = true,
            FirstName = configuration["SeedData:Player1FirstName"] ?? "Player",
            LastName = configuration["SeedData:Player1LastName"] ?? "One",
            ProfilePicturePath = configuration["SeedData:Player1Picture"] ?? "/images/default-avatar.png",
        };

        var password = "123Pa$$word!";
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, DefaultRoles.User);
        }
    }
}
