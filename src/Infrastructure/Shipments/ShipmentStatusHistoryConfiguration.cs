using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipSharp.Domain.Shipments;

namespace ShipSharp.Infrastructure.Shipments;

public class ShipmentStatusHistoryConfiguration : IEntityTypeConfiguration<ShipmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<ShipmentStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.PreviousStatus).HasConversion<int>();
        builder.Property(h => h.CurrentStatus).HasConversion<int>().IsRequired();
        builder.Property(h => h.UpdatedBy).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Timestamp).IsRequired();
    }
}
