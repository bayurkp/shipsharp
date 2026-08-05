using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShipSharp.Application.Auth;
using ShipSharp.Application.Customers;
using ShipSharp.Application.Ports;
using ShipSharp.Application.Shipments;
using ShipSharp.Application.Vessels;

namespace ShipSharp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPortService, PortService>();
        services.AddScoped<IVesselService, VesselService>();
        services.AddScoped<IShipmentService, ShipmentService>();

        return services;
    }
}
