using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entity;

namespace Product.Infrastructure.Configurations;

internal class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
	public void Configure(EntityTypeBuilder<Vendor> builder)
	{
		builder.Property(v => v.BusinessName)
			.HasColumnName("Name")
			.HasMaxLength(50)
			.IsRequired();

		builder.Property(v => v.Address)
			.HasColumnName("Address")
			.HasMaxLength(50)
			.IsRequired();

		builder.Property(v => v.Email)
			.HasColumnName("Email")
			.HasMaxLength(50)
			.IsRequired();

		builder.HasMany(v => v.VendorUsers)
			.WithOne(vu => vu.Vendor)
			.HasForeignKey(vu => vu.VendorId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasMany(v => v.VendorFacilities)
			.WithOne(vf => vf.Vendor)
			.HasForeignKey(vf => vf.VendorId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
