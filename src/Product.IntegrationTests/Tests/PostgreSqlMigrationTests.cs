using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Infrastructure;
using Product.Infrastructure.Dependency_Injection;
using Product.Infrastructure.Implementations.Account;
using Product.Infrastructure.Implementations;
using Testcontainers.PostgreSql;

namespace Product.IntegrationTests.Tests;

public class PostgreSqlMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:16").Build();
    public Task InitializeAsync() => _database.StartAsync();
    public async Task DisposeAsync() => await _database.DisposeAsync();

    [Fact]
    public async Task MigrationsAndProductionRegistration_SupportUtcAuditingAndRepeatedUpdates()
    {
        var services = new ServiceCollection();
        services.AddScoped<IUserPrincipalService, FakeUserPrincipalService>();
        services.AddDataAccessLayer(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = _database.GetConnectionString() }).Build());
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
        await context.Database.MigrateAsync();
        Assert.Single(context.Database.GetMigrations());
        Assert.False(context.Database.HasPendingModelChanges());
        var passwordHash = new PasswordHasher().HashThePassword("migration-test-password");
        var admin = new Administrator { Email = "admin@example.com", UserType = UserType.SuperAdmin, PasswordHash = passwordHash };
        context.Administrators.Add(admin);
        context.SaveChanges(); // Same path as the console seeder.
        var id = admin.Id;
        context.ChangeTracker.Clear();
        admin = await context.Administrators.SingleAsync();
        Assert.True(admin.Id > 0);
        Assert.Equal(DateTimeKind.Utc, admin.CreatedAt.Kind);
        Assert.True(admin.CreatedAt > DateTime.UtcNow.AddMinutes(-5));
        Assert.Equal(passwordHash, admin.PasswordHash);
        admin.FirstName = "Updated";
        await context.SaveChangesAsync();
        await context.Database.MigrateAsync();
        context.ChangeTracker.Clear();
        admin = await context.Administrators.SingleAsync();
        Assert.Equal(id, admin.Id);
        Assert.Equal(DateTimeKind.Utc, admin.UpdatedAt!.Value.Kind);
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
    }

    [Theory]
    [InlineData("mixed", SortOrder.Ascending, "A Mixed", "B MIXED")]
    [InlineData("mixed", SortOrder.Descending, "B MIXED", "A Mixed")]
    [InlineData("%_", SortOrder.Ascending, "Literal %_", null)]
    public async Task VendorSearch_PreservesCaseInsensitiveLiteralMatching(
        string search, SortOrder order, string first, string? second)
    {
        var services = new ServiceCollection();
        services.AddScoped<IUserPrincipalService, FakeUserPrincipalService>();
        services.AddDistributedMemoryCache();
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddDataAccessLayer(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = _database.GetConnectionString() }).Build());
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        foreach (var name in new[] { "A Mixed", "B MIXED", "Literal %_", "Unrelated" })
            context.Vendors.Add(new Vendor { BusinessName = name, Address = "Address", Email = "vendor@example.com" });
        await context.SaveChangesAsync();
        var result = await scope.ServiceProvider.GetRequiredService<IVendorRepository>()
            .GetVendorsQuery(search, order, 10, 1, CancellationToken.None);
        Assert.Equal(second is null ? 1 : 2, result.TotalCount);
        Assert.Equal(first, result.Items[0].BusinessName);
        if (second is not null) Assert.Equal(second, result.Items[1].BusinessName);
    }
}
