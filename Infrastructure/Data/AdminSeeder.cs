using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data
{
    /// <summary>
    /// One-time bootstrap: creates the very first Admin account from config so there's
    /// someone who can log in and create everyone else. Does nothing once any Staff exists,
    /// and does nothing if the config values aren't set. Safe to leave wired up permanently.
    /// </summary>
    public static class AdminSeeder
    {
        public static async Task SeedAsync(AppDbContext db, IConfiguration configuration)
        {
            if (await db.Staffs.AnyAsync()) return;

            var email = configuration["InitialAdmin:Email"];
            var password = configuration["InitialAdmin:Password"];
            var name = configuration["InitialAdmin:Name"] ?? "Admin";
            var surname = configuration["InitialAdmin:Surname"] ?? "User";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

            db.Staffs.Add(new Staff
            {
                StaffId = "DGC-0001",
                Name = name,
                Surname = surname,
                Email = email,
                PhoneNumber = configuration["InitialAdmin:PhoneNumber"] ?? "0000000000",
                JobTitle = "Administrator",
                Role = StaffRole.Admin,
                IsActive = true,
                MustChangePassword = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            });

            await db.SaveChangesAsync();
            Console.WriteLine($"Seeded initial admin account: {email}");
        }
    }
}
