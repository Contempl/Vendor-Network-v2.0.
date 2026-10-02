using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Infrastructure;
using Product.Infrastructure.Implementations.Account;
using Testcontainers.MsSql;

namespace Product.IntegrationTests.Tests;

public class SqlServerMigrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _database =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public Task InitializeAsync() => _database.StartAsync();

    public async Task DisposeAsync() => await _database.DisposeAsync();

    [Fact]
    public async Task UpgradeAndRollback_PreserveExistingBusinessesAndAdministrator()
    {
        var connectionString = new SqlConnectionStringBuilder(_database.GetConnectionString())
        {
            InitialCatalog = "vendor_network_migration_tests"
        }.ConnectionString;
        var services = new ServiceCollection();
        services.AddScoped<IUserPrincipalService, FakeUserPrincipalService>();
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var migrations = context.Database.GetMigrations().ToArray();
        Assert.EndsWith("_UpgradeToEfCore10", migrations[^1]);
        var previousMigration = migrations[^2];
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(previousMigration);

        var vendor = new Vendor
        {
            BusinessName = "Existing vendor",
            Address = "Vendor address",
            Email = "vendor@example.com"
        };
        var businessOperator = new Operator
        {
            BusinessName = "Existing operator",
            Address = "Operator address",
            Email = "operator@example.com",
            Occupation = "Energy"
        };
        var passwordHash = new PasswordHasher().HashThePassword("test-password");
        context.Vendors.Add(vendor);
        context.Operators.Add(businessOperator);
        context.Administrators.Add(new Administrator
        {
            Email = "admin@example.com",
            UserType = UserType.SuperAdmin,
            PasswordHash = passwordHash
        });
        await context.SaveChangesAsync();

        await context.Database.MigrateAsync();
        context.ChangeTracker.Clear();
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.Equal("Existing vendor", (await context.Vendors.SingleAsync()).BusinessName);
        Assert.Equal("Energy", (await context.Operators.SingleAsync()).Occupation);
        var administrator = await context.Administrators.SingleAsync();
        Assert.Equal(UserType.SuperAdmin, administrator.UserType);
        Assert.Equal(passwordHash, administrator.PasswordHash);

        await migrator.MigrateAsync(previousMigration);
        context.ChangeTracker.Clear();
        Assert.Equal(vendor.Id, (await context.Vendors.SingleAsync()).Id);
        Assert.Equal(businessOperator.Id, (await context.Operators.SingleAsync()).Id);
        Assert.Equal(passwordHash, (await context.Administrators.SingleAsync()).PasswordHash);
    }
}
