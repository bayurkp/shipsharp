using Microsoft.EntityFrameworkCore;
using ShipSharp.Application.Common.Interfaces;
using ShipSharp.Domain.Ports;
using ShipSharp.Domain.Users;
using ShipSharp.Domain.Vessels;

namespace ShipSharp.Infrastructure.Data;

public class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordService passwordService)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = passwordService.HashPassword("Admin@123"),
                FullName = "System Administrator",
                Role = UserRole.Admin
            };

            var operatorUser = new User
            {
                Username = "operator",
                PasswordHash = passwordService.HashPassword("Operator@123"),
                FullName = "Logistics Operator",
                Role = UserRole.Operator
            };

            await context.Users.AddRangeAsync(adminUser, operatorUser);
        }

        if (!await context.Ports.AnyAsync())
        {
            var ports = new List<Port>
            {
                new() { Name = "Port of Surabaya (Tanjung Perak)", Code = "SUB", Country = "Indonesia" },
                new() { Name = "Port of Jakarta (Tanjung Priok)", Code = "JKT", Country = "Indonesia" },
                new() { Name = "Port of Singapore", Code = "SIN", Country = "Singapore" },
                new() { Name = "Port Klang", Code = "PKL", Country = "Malaysia" }
            };

            await context.Ports.AddRangeAsync(ports);
        }

        if (!await context.Vessels.AnyAsync())
        {
            var vessels = new List<Vessel>
            {
                new() { Name = "MV ShipSharp One", IMONumber = "IMO9123456", Flag = "Indonesia", Capacity = 50000, IsActive = true },
                new() { Name = "MV ShipSharp Two", IMONumber = "IMO9876543", Flag = "Singapore", Capacity = 75000, IsActive = true }
            };

            await context.Vessels.AddRangeAsync(vessels);
        }

        await context.SaveChangesAsync();
    }
}
