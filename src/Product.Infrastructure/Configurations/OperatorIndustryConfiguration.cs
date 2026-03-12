using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entity;

namespace Product.Infrastructure.Configurations;

internal class OperatorIndustryConfiguration : IEntityTypeConfiguration<OperatorIndustry>
{
	public void Configure(EntityTypeBuilder<OperatorIndustry> builder)
	{
		builder.ToTable("Industries");

		builder.HasKey(oi => oi.Id);

		builder.Property(oi => oi.Name)
			.HasColumnName("Name")
			.HasMaxLength(40)
			.IsRequired();

		builder.Property(oi => oi.Address)
			.HasColumnName("Address")
			.HasMaxLength(50)
			.IsRequired();

		builder.Property(oi => oi.Latitude)
			.HasColumnName("Latitude")
			.HasPrecision(10, 8)
			.IsRequired();

		builder.Property(oi => oi.Longitude)
			.HasColumnName("Longitude")
			.HasPrecision(11, 8)
			.IsRequired();

		builder.HasOne(oi => oi.Operator)
			.WithMany(op => op.Industries)
			.HasForeignKey(oi => oi.OperatorId);
	}
}
