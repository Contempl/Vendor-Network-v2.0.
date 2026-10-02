using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Product.DataMigration;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Infrastructure;
using Product.Infrastructure.Implementations.Account;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace Product.IntegrationTests.Tests;

public class DataTransferTests : IAsyncLifetime
{
    private readonly MsSqlContainer _source = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private readonly PostgreSqlContainer _target = new PostgreSqlBuilder("postgres:16").Build();
    private readonly byte[] _modernHash = new PasswordHasher().HashThePassword("modern-password");
    private readonly byte[] _legacyHash = SHA512.HashData(Encoding.UTF8.GetBytes("legacy-password"));
    private string SourceConnection => new SqlConnectionStringBuilder(_source.GetConnectionString()) { InitialCatalog = "TransferTest" }.ConnectionString;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_source.StartAsync(), _target.StartAsync());
        await using var master = new SqlConnection(_source.GetConnectionString());
        await master.OpenAsync();
        await new SqlCommand("CREATE DATABASE TransferTest", master).ExecuteNonQueryAsync();
        await using var source = new SqlConnection(SourceConnection);
        await source.OpenAsync();
        await using var stream = typeof(DataTransferTests).Assembly.GetManifestResourceStream(
            "Product.IntegrationTests.Fixtures.sql-server-baseline.sql")!;
        using var script = new StreamReader(stream);
        foreach (var batch in Regex.Split(await script.ReadToEndAsync(), @"^GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            if (!string.IsNullOrWhiteSpace(batch))
                await new SqlCommand(batch, source) { CommandTimeout = 120 }.ExecuteNonQueryAsync();
        await using var seed = new SqlCommand("""
            SET IDENTITY_INSERT Businesses ON;
            INSERT Businesses (Id, Name, Address, Email, BusinessType, CreatedAt, CreatedBy) VALUES
              (42, N'Existing Vendor', N'Address', 'vendor@example.com', 'Vendor', '2025-01-01', 0),
              (43, N'Existing Operator', N'Address', 'operator@example.com', 'Operator', '2025-01-01', 0);
            SET IDENTITY_INSERT Businesses OFF;
            SET IDENTITY_INSERT [User] ON;
            INSERT [User] (Id, Email, Password, UserType, CreatedAt, CreatedBy, BusinessId) VALUES
              (100, 'admin@example.com', @modernHash, 'Admin', '2025-01-01', 0, NULL),
              (101, 'vendor-user@example.com', @legacyHash, 'VendorUser', '2025-01-01', 0, 42),
              (102, 'operator-user@example.com', NULL, '0', '2025-01-01', 0, 43),
              (103, 'local-admin@example.com', @modernHash, 'Admin', '2025-01-01', 0, 42);
            SET IDENTITY_INSERT [User] OFF;
            INSERT Administrators (Id) VALUES (100), (103);
            INSERT VendorUsers (Id, VendorId) VALUES (101, 42);
            INSERT OperatorUsers (Id, OperatorId) VALUES (102, 43);
            INSERT Industries (Name, Address, Latitude, Longitude, OperatorId, CreatedAt, CreatedBy)
              VALUES ('Industry', 'Address', 55.75, 37.61, 43, '2025-01-01', 100);
            INSERT VendorsFacilities (Name, Location, Latitude, Longitude, VendorId, RadiusOfWork, CreatedAt, CreatedBy)
              VALUES ('Facility', 'Address', 55.75, 37.61, 42, 10, '2025-01-01', 100);
            INSERT VendorsFacilityServices (Name, VendorFacilityId, CreatedAt, CreatedBy)
              VALUES ('Service', 1, '2025-01-01', 100);
            INSERT Invites (Status, CreatedAt, CreatedBy, ExpiresAt, InvitedUserId, SenderId)
              VALUES ('Sent', '2025-01-01', 100, '2030-01-01', 101, 100);
            INSERT RefreshTokens (Token, UserId, ExpiresAt, Revoked)
              VALUES ('existing-refresh-token', 101, '2030-01-01', 0);
            """, source);
        seed.Parameters.AddWithValue("modernHash", _modernHash);
        seed.Parameters.AddWithValue("legacyHash", _legacyHash);
        await seed.ExecuteNonQueryAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _source.DisposeAsync();
        await _target.DisposeAsync();
    }

    private AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_target.GetConnectionString()).Options,
        new ServiceCollection().BuildServiceProvider());

    [Fact]
    public async Task Transfer_PreservesHashesRelationshipsAndIds_AndAppliesMissingLegacyRoleUpgrade()
    {
        var counts = await SqlServerToPostgres.TransferAsync(SourceConnection, _target.GetConnectionString());
        Assert.Equal(10, counts.Count);
        Assert.Equal(4, counts["User"]);
        await using var context = CreateContext();
        var administrator = await context.Administrators.SingleAsync(a => a.Id == 100);
        Assert.Equal(UserType.SuperAdmin, administrator.UserType);
        Assert.Equal(UserType.Admin, (await context.Administrators.SingleAsync(a => a.Id == 103)).UserType);
        Assert.Equal(_modernHash, administrator.PasswordHash);
        Assert.True(new PasswordHasher().ValidatePassword("modern-password", administrator.PasswordHash!));
        var vendorUser = await context.VendorUsers.Include(u => u.Vendor).SingleAsync();
        Assert.Equal(101, vendorUser.Id);
        Assert.Equal(42, vendorUser.VendorId);
        Assert.Equal("Existing Vendor", vendorUser.Vendor.BusinessName);
        Assert.Equal(_legacyHash, vendorUser.PasswordHash);
        Assert.True(new PasswordHasher().ValidatePassword("legacy-password", vendorUser.PasswordHash!));
        Assert.Equal(UserType.OperatorUser, (await context.OperatorUsers.SingleAsync()).UserType);
        Assert.Equal(DateTimeKind.Utc, vendorUser.CreatedAt.Kind);
        Assert.Equal(101, (await context.RefreshTokens.SingleAsync()).UserId);
        Assert.Equal(100, (await context.Invites.SingleAsync()).SenderId);
        Assert.Equal(42, (await context.VendorFacilities.Include(f => f.Services).SingleAsync()).VendorId);
        Assert.Single((await context.VendorFacilities.Include(f => f.Services).SingleAsync()).Services);
        Assert.Equal(43, (await context.OperatorIndustries.SingleAsync()).OperatorId);
        var newAdmin = new Administrator { Email = "new@example.com", UserType = UserType.SuperAdmin };
        context.Administrators.Add(newAdmin);
        await context.SaveChangesAsync();
        Assert.True(newAdmin.Id > 103);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SqlServerToPostgres.TransferAsync(SourceConnection, _target.GetConnectionString()));
        Assert.Equal(5, await context.Set<User>().CountAsync());
        await using var source = new SqlConnection(SourceConnection);
        await source.OpenAsync();
        Assert.Equal("Admin", await new SqlCommand("SELECT UserType FROM [User] WHERE Id = 100", source).ExecuteScalarAsync());
    }

    [Fact]
    public async Task Transfer_RollsBackAllTables_WhenALateForeignKeyIsInvalid()
    {
        await using var source = new SqlConnection(SourceConnection);
        await source.OpenAsync();
        await new SqlCommand("ALTER TABLE Invites NOCHECK CONSTRAINT ALL; UPDATE Invites SET SenderId = 9999;", source).ExecuteNonQueryAsync();
        await Assert.ThrowsAsync<PostgresException>(() =>
            SqlServerToPostgres.TransferAsync(SourceConnection, _target.GetConnectionString()));
        await using var context = CreateContext();
        Assert.Equal(0, await context.Businesses.CountAsync());
        Assert.Equal(0, await context.Set<User>().CountAsync());
        Assert.Equal(0, await context.VendorFacilities.CountAsync());
    }
}
