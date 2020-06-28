using BlazorTemplate.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorTemplate.Server.Data
{
    public class SeedData
    {
        private readonly ApplicationDbContext ctx;
        public SeedData(ApplicationDbContext dbContext)
        {
            ctx = dbContext;
        }

        public async Task CreateUserAndRoles(IServiceProvider serviceProvider)
        {
            if (ctx.Users.Any())
            {
                return;
            }

            // Initializing Custom Roles
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string[] roleNames = { "Administrators", "Users" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create Administrator
            ApplicationUser admin = await UserManager.FindByEmailAsync("admin@example.com");
            if (admin == null)
            {
                admin = new ApplicationUser()
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    CustomClaim = "AdminClaim"
                };
                await UserManager.CreateAsync(admin, "Qwerty1234#");
            }
            // Add Roles
            await UserManager.AddToRoleAsync(admin, "Administrators");
            await UserManager.AddToRoleAsync(admin, "Users");

            // Create User
            ApplicationUser user = await UserManager.FindByEmailAsync("user@example.com");

            if (user == null)
            {
                user = new ApplicationUser()
                {
                    UserName = "user@example.com",
                    Email = "user@example.com",
                    CustomClaim = "UserClaim"

                };
                await UserManager.CreateAsync(user, "Qwerty1234#");
            }
            await UserManager.AddToRoleAsync(user, "Users");
        }

    }
}
