using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Infrastructure.Implementations.Account;

namespace Product.IntegrationTests.Tests;

public class AccountIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AccountIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/Admin/Login")]
    [InlineData("/Account/Login")]
    public async Task Login_UpgradesLegacyPasswordHash(string route)
    {
        var email = $"legacy-{Guid.NewGuid():N}".Substring(0, 23) + "@example.com";
        var legacyHash = SHA512.HashData(Encoding.UTF8.GetBytes("test-password"));
        await _factory.SeedAsync(async context =>
        {
            context.Administrators.Add(new Administrator
            {
                Email = email,
                UserType = UserType.SuperAdmin,
                PasswordHash = legacyHash
            });
            await context.SaveChangesAsync();
        });

        using var response = await _client.PostAsJsonAsync(route,
            new { email, password = "test-password" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await _factory.SeedAsync(async context =>
        {
            var storedHash = await context.Administrators
                .Where(admin => admin.Email == email)
                .Select(admin => admin.PasswordHash)
                .SingleAsync();
            Assert.NotNull(storedHash);
            Assert.NotEqual(legacyHash, storedHash);
            Assert.True(new PasswordHasher().ValidatePassword("test-password", storedHash));
            Assert.False(new PasswordHasher().NeedsRehash(storedHash));
        });
    }

    [Fact]
    public async Task Refresh_AllowsAnonymousRequest_AndRejectsUnknownToken()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/refresh")
        {
            Content = JsonContent.Create(new { refreshToken = "unknown-token" })
        };
        request.Headers.Add("X-Test-Anonymous", "true");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AdminLogin_RefreshesWithoutAccessToken_AndRotatesRefreshToken()
    {
        await _factory.SeedAsync(async context =>
        {
            context.Administrators.Add(new Administrator
            {
                Email = "refresh-test@example.com",
                UserType = UserType.SuperAdmin,
                PasswordHash = new PasswordHasher().HashThePassword("test-password")
            });
            await context.SaveChangesAsync();
        });

        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/Admin/Login")
        {
            Content = JsonContent.Create(new { email = "refresh-test@example.com", password = "test-password" })
        };
        loginRequest.Headers.Add("X-Test-Anonymous", "true");
        using var loginResponse = await _client.SendAsync(loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginTokens = await loginResponse.Content.ReadFromJsonAsync<TokenDto>();
        Assert.False(string.IsNullOrWhiteSpace(loginTokens?.RefreshToken));
        var oldToken = loginTokens.RefreshToken;

        using var request = new HttpRequestMessage(HttpMethod.Post, "/refresh")
        {
            Content = JsonContent.Create(new { refreshToken = oldToken })
        };
        request.Headers.Add("X-Test-Anonymous", "true");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = await response.Content.ReadFromJsonAsync<TokenDto>();
        Assert.False(string.IsNullOrWhiteSpace(tokens?.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens?.RefreshToken));
        Assert.NotEqual(oldToken, tokens.RefreshToken);

        await _factory.SeedAsync(async context =>
        {
            Assert.True(await context.RefreshTokens.AnyAsync(t => t.Token == oldToken && t.Revoked));
            Assert.True(await context.RefreshTokens.AnyAsync(t => t.Token == tokens.RefreshToken && !t.Revoked));
        });
    }
}
