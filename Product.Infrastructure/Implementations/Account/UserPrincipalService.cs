using System.Security.Claims;
using Product.Application.ServiceInterfaces;

namespace Product.Infrastructure.Implementations.Account;

public class UserPrincipalService : IUserPrincipalService
{
    public int? UserId { get; set; }
    public string? UserType { get; set; } //enum
    public int? BusinessId { get; set; }
    

    public UserPrincipalService(ClaimsPrincipal claimsPrincipal)
    {
        UserId = GetIntClaim(claimsPrincipal.Claims, "userId");
        BusinessId = GetIntClaim(claimsPrincipal.Claims, "businessId");
        UserType = GetClaim(claimsPrincipal.Claims, "userType");
    }
    private string? GetClaim(IEnumerable<Claim> claims, string claimName) => claims.SingleOrDefault(c => c.Type == claimName)?.Value;
    private int? GetIntClaim(IEnumerable<Claim> claims, string claimName) => int.TryParse(GetClaim(claims, claimName), out var value) ? value : null;
}
