using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Product.Domain.Entity;
using Product.Infrastructure.Configurations;
using Product.Infrastructure.Interceptors;

namespace Product.Infrastructure;

public class AppDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider) : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new DateInterceptor(_serviceProvider));
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
    }


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
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
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
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options, null);
    }
}