using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;

namespace MovieBookingSystem.Services
{
    public class SeedServices
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedServices>>();

                try
                {
                    logger.LogInformation("Seeding database...");
                    await context.Database.EnsureCreatedAsync();

                    logger.LogInformation("Seeding roles...");
                    await AddRoleAsync(roleManager, "Admin");
                    await AddRoleAsync(roleManager, "User");

                    logger.LogInformation("Seeding admin user...");
                    var adminEmail = "admin@iiuc.com";

                    if (await userManager.FindByEmailAsync(adminEmail) == null)
                    {
                        var adminUser = new Users
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            NormalizedUserName = adminEmail.ToUpper(),
                            FullName = "Admin User",
                            NormalizedEmail = adminEmail.ToUpper(),
                            EmailConfirmed = true,
                            SecurityStamp = Guid.NewGuid().ToString()
                        };

                        var result = await userManager.CreateAsync(adminUser, "Admin@123");

                        if (result.Succeeded)
                        {
                            logger.LogInformation("Admin user created successfully.");
                            await userManager.AddToRoleAsync(adminUser, "Admin");
                        }
                        else
                        {
                            logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
        }

        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
