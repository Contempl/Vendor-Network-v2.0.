using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entity;

namespace Product.Infrastructure.Configurations;

internal class OperatorUserConfiguration : IEntityTypeConfiguration<OperatorUser>
{
	public void Configure(EntityTypeBuilder<OperatorUser> builder)
	{
		builder.ToTable("OperatorUsers");

		builder.Property(ou => ou.UserName)
			.HasColumnName("Username")
			.HasMaxLength(50);

		builder.Property(ou => ou.PasswordHash)
			.HasColumnName("Password");

		builder.Property(ou => ou.FirstName)
			.HasColumnName("FirstName")
			.HasMaxLength(50);

		builder.Property(ou => ou.LastName)
			.HasColumnName("LastName")
			.HasMaxLength(50);

		builder.Property(ou => ou.Email)
			.HasMaxLength(50)
			.IsRequired();

		builder.HasOne(ou => ou.Operator)
			.WithMany(o => o.OperatorUsers)
			.HasForeignKey(ou => ou.OperatorId);
	}
}
