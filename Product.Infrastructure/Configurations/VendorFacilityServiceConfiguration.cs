using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entity;

namespace Product.Infrastructure.Configurations;

internal class VendorFacilityServiceConfiguration : IEntityTypeConfiguration<VendorFacilityService>
{
	public void Configure(EntityTypeBuilder<VendorFacilityService> builder)
	{
		builder.ToTable("VendorsFacilityServices");

		builder.HasKey(vfs => vfs.Id);

		builder.Property(vfs => vfs.Name)
			.HasMaxLength(40)
			.IsRequired();

		builder.HasOne(vfs => vfs.VendorFacility)
			.WithMany(vf => vf.Services)
			.HasForeignKey(vfs => vfs.VendorFacilityId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
