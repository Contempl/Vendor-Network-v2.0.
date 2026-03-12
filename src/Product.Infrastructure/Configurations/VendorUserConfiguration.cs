using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entity;

namespace Product.Infrastructure.Configurations;

internal class VendorUserConfiguration : IEntityTypeConfiguration<VendorUser>
{
	public void Configure(EntityTypeBuilder<VendorUser> builder)
	{
		builder.ToTable("VendorUsers");

		builder.Property(vu => vu.UserName)
			.HasColumnName("Username")
			.HasMaxLength(50);

		builder.Property(vu => vu.PasswordHash)
			.HasColumnName("Password");

		builder.Property(vu => vu.FirstName)
			.HasColumnName("FirstName")
			.HasMaxLength(50);

		builder.Property(vu => vu.LastName)
			.HasColumnName("LastName")
			.HasMaxLength(50);

		builder.Property(vu => vu.Email)
			.HasMaxLength(50)
			.IsRequired();

		builder.HasOne(vu => vu.Vendor)
			.WithMany(v => v.VendorUsers)
			.HasForeignKey(vu => vu.VendorId);
	}
}
