using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entity;

namespace Product.Infrastructure.Configurations;

internal class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
	public void Configure(EntityTypeBuilder<Operator> builder)
	{
		builder.Property(v => v.BusinessName)
			.HasColumnName("Name")
			.HasMaxLength(50)
			.IsRequired();

		builder.Property(v => v.Adress)
			.HasColumnName("Address")
			.HasMaxLength(50)
			.IsRequired();

		builder.Property(v => v.Email)
			.HasColumnName("Email")
			.HasMaxLength(50)
			.IsRequired();

		builder.Property(op => op.LogoUrl)
			.HasColumnName("LogoUrl");

		builder.Property(op => op.Occupation)
			.HasColumnName("Occupation")
			.HasMaxLength(50);

		builder.HasMany(op => op.Industries)
			.WithOne(industry => industry.Operator)
			.HasForeignKey(industry => industry.OperatorId);

		builder.HasMany(op => op.OperatorUsers)
			.WithOne(ou => ou.Operator)
			.HasForeignKey(ou => ou.OperatorId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
