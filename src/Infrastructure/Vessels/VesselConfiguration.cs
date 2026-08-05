using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipSharp.Domain.Vessels;

namespace ShipSharp.Infrastructure.Vessels;

public class VesselConfiguration : IEntityTypeConfiguration<Vessel>
{
    public void Configure(EntityTypeBuilder<Vessel> builder)
    {
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => v.IMONumber).IsUnique();
        builder.Property(v => v.Name).HasMaxLength(100).IsRequired();
        builder.Property(v => v.IMONumber).HasMaxLength(20).IsRequired();
        builder.Property(v => v.Flag).HasMaxLength(50).IsRequired();
        builder.Property(v => v.Capacity).HasColumnType("decimal(18,2)").IsRequired();
    }
}
