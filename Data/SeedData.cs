using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniAccountManagementSystem.Data; // Ensure this is correct for your DbContext
using MiniAccountsSystem.Data;
using System; // For Guid.NewGuid() if you choose to generate IDs manually, though Identity handles this
using System.Threading.Tasks;

namespace MiniAccountManagementSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // 1. Apply pending migrations (optional, but good practice for seeding)
            // This ensures your database schema is up-to-date before seeding data.
            // You can remove this if you always run Update-Database manually.
            await context.Database.MigrateAsync();

            // 2. Seed Roles
            string[] roleNames = { "Admin", "User" }; // Define your roles
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName)) // Check if role already exists
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName)); // Create the role
                }
            }

            // 3. Seed an Admin user
            string adminEmail = "admin@yourapp.com"; // Choose a default admin email
            string adminPassword = "AdminPassword123!"; // *IMPORTANT: Use a strong default password and consider changing it later*
            string adminRole = "Admin";

            // Check if the admin user already exists
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // It's common to auto-confirm seeded admin users
                };

                // Create the admin user with the specified password
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // Assign the Admin role to the newly created admin user
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
                else
                {
                    // Log errors if admin user creation fails
                    // In a real application, you'd log these errors more robustly
                    Console.WriteLine("Error creating admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"- {error.Description}");
                    }
                }
            }
        }
    }
}