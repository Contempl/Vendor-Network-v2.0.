using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Product.Application.ServiceInterfaces;
using Product.Domain.Enum;

namespace Product.IntegrationTests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserPrincipalService _userPrincipalService;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserPrincipalService userPrincipalService)
        : base(options, logger, encoder, clock)
    {
        _userPrincipalService = userPrincipalService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.TryGetValue("X-Test-UserId", out var userIdVal) &&
            int.TryParse(userIdVal, out var userId))
            ((FakeUserPrincipalService)_userPrincipalService).UserId = userId;

        if (Request.Headers.TryGetValue("X-Test-BusinessId", out var businessIdValue) &&
            int.TryParse(businessIdValue, out var bizId))
            ((FakeUserPrincipalService)_userPrincipalService).BusinessId = bizId;

        UserType userType = UserType.VendorUser; 
        if (Request.Headers.TryGetValue("X-Test-Role", out var roleVal) &&
            Enum.TryParse<UserType>(roleVal, out var parsedRole))
        {
            userType = parsedRole;
            ((FakeUserPrincipalService)_userPrincipalService).UserType = userType;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userPrincipalService.UserId.ToString()!),
            new Claim(ClaimTypes.Role, userType.ToString()),
        };
        
        var identity = new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}