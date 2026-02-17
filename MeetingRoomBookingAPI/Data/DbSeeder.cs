using MeetingRoomBookingAPI.Domain.Entities;
using MeetingRoomBookingAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoomBookingAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            // Apply migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }

            // Roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            // Admin User
            var adminEmail = "admin@local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Admin123!");
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            
            adminUser.Profile = new UserProfile { FullName = "System Admin", Department = "IT" };
            await context.SaveChangesAsync();

            // Regular Users
            string[] users = { "user1@local", "user2@local" };
            foreach (var email in users)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                    await userManager.CreateAsync(user, "User123!");
                    await userManager.AddToRoleAsync(user, "User");
                    
                    user.Profile = new UserProfile { FullName = email.Split('@')[0], Department = "General" };
                    await context.SaveChangesAsync();
                }
            }

            // Rooms
            if (!context.Rooms.Any())
            {
                context.Rooms.AddRange(new List<Room>
                {
                    new Room { Name = "Conference Room A", Capacity = 10, Location = "Floor 1", HasProjector = true },
                    new Room { Name = "Meeting Room B", Capacity = 5, Location = "Floor 1", HasProjector = false },
                    new Room { Name = "Grand Hall", Capacity = 50, Location = "Floor 2", HasProjector = true }
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
