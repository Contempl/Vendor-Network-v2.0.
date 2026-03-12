using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entity;

namespace Product.Infrastructure.Configurations;

internal class VendorFacilityConfiguration : IEntityTypeConfiguration<VendorFacility>
{
	public void Configure(EntityTypeBuilder<VendorFacility> builder)
	{
		builder.ToTable("VendorsFacilities");

		builder.HasKey(vf => vf.Id);

		builder.Property(vf => vf.Name)
			.HasMaxLength(50)
			.IsRequired();

		builder.Property(vf => vf.Location)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(vf => vf.Longitude)
			.HasPrecision(11, 8)
			.IsRequired();

		builder.Property(vf => vf.Latitude)
			.HasPrecision(10, 8)
			.IsRequired();

		builder.HasOne(vf => vf.Vendor)
			.WithMany(v => v.VendorFacilities)
			.HasForeignKey(vf => vf.VendorId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(vf => vf.Services)
			.WithOne(vfs => vfs.VendorFacility)
			.HasForeignKey(vfs => vfs.VendorFacilityId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
