using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ShipSharp.Application.Common.Interfaces;
using ShipSharp.Domain.Customers;
using ShipSharp.Domain.Ports;
using ShipSharp.Domain.Shipments;
using ShipSharp.Domain.Users;
using ShipSharp.Domain.Vessels;
using ShipSharp.Infrastructure.Customers;
using ShipSharp.Infrastructure.Data;
using ShipSharp.Infrastructure.Ports;
using ShipSharp.Infrastructure.Services;
using ShipSharp.Infrastructure.Shipments;
using ShipSharp.Infrastructure.Users;
using ShipSharp.Infrastructure.Vessels;

namespace ShipSharp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=shipsharp.db";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (connectionString.Contains("Data Source=") || connectionString.EndsWith(".db"))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddMemoryCache();
        services.AddScoped<PortRepository>();
        services.AddScoped<IPortRepository>(sp =>
            new CachedPortRepository(
                sp.GetRequiredService<PortRepository>(),
                sp.GetRequiredService<IMemoryCache>()));
        services.AddScoped<IVesselRepository, VesselRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();

        return services;
    }
}
