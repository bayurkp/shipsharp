using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipSharp.Domain.Shipments;

namespace ShipSharp.Infrastructure.Shipments;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.TrackingNumber).IsUnique();
        builder.Property(s => s.TrackingNumber).HasMaxLength(30).IsRequired();
        builder.Property(s => s.Status).HasConversion<int>().IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(500);

        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.OriginPort)
            .WithMany()
            .HasForeignKey(s => s.OriginPortId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.DestinationPort)
            .WithMany()
            .HasForeignKey(s => s.DestinationPortId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Vessel)
            .WithMany()
            .HasForeignKey(s => s.VesselId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.StatusHistories)
            .WithOne(h => h.Shipment)
            .HasForeignKey(h => h.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
