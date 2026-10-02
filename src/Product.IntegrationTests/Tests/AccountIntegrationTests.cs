using System.Net;
using System.Net.Http.Json;
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
