using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entity;

namespace Product.Infrastructure.Configurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
	public void Configure(EntityTypeBuilder<Business> builder)
	{
		builder.ToTable("Businesses");

		builder.HasKey(business => business.Id);

		builder.Property(business => business.BusinessName)
			.HasColumnName("BusinessName")
			.HasMaxLength(50)
			.IsRequired();

		builder.Property(business => business.Adress)
			.HasColumnName("Address")
			.HasMaxLength(50)
			.IsRequired();

		builder.Property(business => business.Email)
			.HasColumnName("Email")
			.HasMaxLength(50)
			.IsRequired();

		builder.HasDiscriminator<string>("BusinessType")
			.HasValue<Vendor>("Vendor")
			.HasValue<Operator>("Operator");
	}
}
