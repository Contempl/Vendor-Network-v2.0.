using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entity;

namespace Product.Infrastructure.Configurations;

public class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
	public void Configure(EntityTypeBuilder<Administrator> builder)
	{
		builder.ToTable("Administrators");

		builder.Property(admin => admin.UserName)
			.HasColumnName("Username")
			.HasMaxLength(50);

		builder.Property(admin => admin.PasswordHash)
			.HasColumnName("Password");

		builder.HasMany(admin => admin.Invites)
			.WithOne(invite => invite.Admin)
			.HasForeignKey(invite => invite.AdminId);

		builder.Property(admin => admin.FirstName)
			.HasColumnName("FirstName")
			.HasMaxLength(50);

		builder.Property(admin => admin.LastName)
			.HasColumnName("LastName")
			.HasMaxLength(50);

		builder.Property(admin => admin.Email)
			.HasColumnName("Email")
			.HasMaxLength(50)
			.IsRequired();
	}
}
