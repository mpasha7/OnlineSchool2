using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace OnlineSchool2.Models
{
    public static class IdentitySeedData
    {
        public static void CreateAdminAccount(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            CreateAdminAccountAsync(serviceProvider, configuration).Wait();
        }

        public static async Task CreateAdminAccountAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!(await userManager.GetUsersInRoleAsync("Admin")).Any())
            {
                string[] users = new string[] { "AdminUser", "FirstCoach", "SecondCoach", "FirstStudent", "SecondStudent" };
                foreach (string item in users)
                {
                    string? username = configuration[$"Data:{item}:Name"];
                    string? email = configuration[$"Data:{item}:Email"];
                    string? password = configuration[$"Data:{item}:Password"];
                    string? role = configuration[$"Data:{item}:Role"];

                    if (username != null && email != null && password != null && role != null
                        && (await userManager.FindByNameAsync(username)) == null)
                    {
                        if (await roleManager.FindByNameAsync(role) == null)
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                        IdentityUser user = new IdentityUser
                        {
                            UserName = username,
                            Email = email
                        };
                        IdentityResult result = await userManager.CreateAsync(user, password);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, role);
                        }
                    }
                }
            }
        }
    }
}