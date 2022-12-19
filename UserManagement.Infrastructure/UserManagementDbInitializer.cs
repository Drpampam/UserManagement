using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Models;

namespace UserManagement.Infrastructure
{
    public class UserManagementDbInitializer
    {
        public static async Task Seed(IApplicationBuilder builder)
        {
            using var serviceScope = builder.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetService<UserManagementDbContext>();
            string filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, @"UserManagement.Infrastructure\Data\");
            if (await context.Database.EnsureCreatedAsync()) return;

            if (!context.Roles.Any())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var readText = await File.ReadAllTextAsync(filePath + "Roles.json");
                List<IdentityRole> Roles = JsonConvert.DeserializeObject<List<IdentityRole>>(readText);
                foreach (var role in Roles)
                {
                    await roleManager.CreateAsync(role);
                }
            }
            if (!context.AppUser.Any())
            {
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var readText = await File.ReadAllTextAsync(filePath + "Users.json");
                List<AppUser> users = JsonConvert.DeserializeObject<List<AppUser>>(readText);
                users.ForEach(delegate (AppUser user) {
                    userManager.CreateAsync(user, "Jaspino2_06$");
                    userManager.AddToRoleAsync(user, UserRole.Admin.ToString());
                    context.AppUser.AddAsync(user);
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
