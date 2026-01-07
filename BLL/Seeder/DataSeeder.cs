using BLL.Settings;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Seeder
{
    public static class DataSeeder
    {
        // ---------------- Seed Roles & Admin ----------------

        public static async Task SeedRolesAndAdminAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            try
            {
                // 1️⃣ Seed Roles
                string[] roles = { "Admin", "Teacher", "Student", "Parent" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                        Log.Information("Role {Role} created", role);
                    }
                }

                // 2️⃣ Seed Admin User from appsettings
                var adminSettings = configuration.GetSection("AdminUser").Get<AdminUserSettings>();

                if (adminSettings == null)
                {
                    Log.Warning("AdminUser settings missing in configuration. Skipping admin seeding.");
                    return;
                }

                var adminUser = await userManager.FindByEmailAsync(adminSettings.Email);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminSettings.Email,
                        Email = adminSettings.Email,
                        FullName = adminSettings.FullName,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, adminSettings.Password);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Log.Information("Default admin user created: {Email}", adminSettings.Email);
                    }
                    else
                    {
                        Log.Error("Failed to create admin user: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    Log.Information("Admin user already exists: {Email}", adminSettings.Email);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error seeding roles and admin user");
            }
        }
    }
}
