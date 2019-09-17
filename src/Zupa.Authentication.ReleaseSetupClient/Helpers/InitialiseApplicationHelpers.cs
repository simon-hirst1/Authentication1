using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Zupa.Authentication.Common;
using Zupa.Authentication.ReleaseSetupClient.Models;

namespace Zupa.Authentication.ReleaseSetupClient.Helpers
{
    internal class InitialiseApplicationHelpers
    {
        internal static async Task CreateUserAsync(UserModel user, UserManager<IdentityUser> userManager)
        {
            var identityUser = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = user.Email,
                Email = user.Email,
                EmailConfirmed = true
            };
            Console.WriteLine($"Adding User: {user.Email}");
            var result = await userManager.CreateAsync(identityUser, user.Password);
            if (!result.Succeeded)
            {
                Console.Error.WriteLine($"Failed to create user {user.Email}");
                foreach (var e in result.Errors)
                    Console.Error.WriteLine($"{e.Code}: {e.Description}");
            }

            if (user.IsAdmin)
                await userManager.AddToRoleAsync(identityUser, RoleConstants.AdminRole);
        }

        internal static async Task CreateAdminRoleAsync (RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync(RoleConstants.AdminRole).Result)
            {
                var role = new IdentityRole { Name = RoleConstants.AdminRole };
                await roleManager.CreateAsync(role);
            }
        }
    }
}
