using Microsoft.EntityFrameworkCore;
using ShipSharp.Domain.Customers;
using ShipSharp.Domain.Ports;
using ShipSharp.Domain.Shipments;
using ShipSharp.Domain.Users;
using ShipSharp.Domain.Vessels;

namespace ShipSharp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Port> Ports => Set<Port>();
    public DbSet<Vessel> Vessels => Set<Vessel>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentStatusHistory> ShipmentStatusHistories => Set<ShipmentStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
