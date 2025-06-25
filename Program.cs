using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity; // For UserManager, RoleManager
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; // For GetRequiredService, CreateScope
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniAccountManagementSystem.Data; // For ApplicationDbContext and SeedData
using MiniAccountsSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniAccountManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build(); // Build the host

            // This block ensures that services are available for seeding
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Get the required services from the DI container
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    // Call the seeding method
                    await SeedData.InitializeAsync(context, userManager, roleManager);
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during seeding
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            await host.RunAsync(); // Run the application
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}