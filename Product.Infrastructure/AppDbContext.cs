using Microsoft.EntityFrameworkCore;
using Product.Domain.Entity;
using Product.Infrastructure.Configurations;

namespace Product.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new AdministratorConfiguration());
		modelBuilder.ApplyConfiguration(new BusinessConfiguration());
		modelBuilder.ApplyConfiguration(new InviteConfiguration());
		modelBuilder.ApplyConfiguration(new OperatorConfiguration());
		modelBuilder.ApplyConfiguration(new OperatorIndustryConfiguration());
		modelBuilder.ApplyConfiguration(new OperatorUserConfiguration());
		modelBuilder.ApplyConfiguration(new UserConfiguration());
		modelBuilder.ApplyConfiguration(new VendorConfiguration());
		modelBuilder.ApplyConfiguration(new VendorFacilityConfiguration());
		modelBuilder.ApplyConfiguration(new VendorFacilityServiceConfiguration());
		modelBuilder.ApplyConfiguration(new VendorUserConfiguration());
	}

	public DbSet<Administrator> Administrators { get; set; }
    public DbSet<Operator> Operators { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VendorUser> VendorUsers { get; set; }
    public DbSet<OperatorUser> OperatorUsers { get; set; }
    public DbSet<OperatorIndustry> OperatorIndustries { get; set; }
    public DbSet<VendorFacility> VendorFacilities { get; set; }
    public DbSet<VendorFacilityService> VendorFacilityServices { get; set; }
    public DbSet<Invite> Invites { get; set; }
    public DbSet<Business> Businesses { get; set; }
}
