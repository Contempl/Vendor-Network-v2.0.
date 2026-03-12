using System.Security.Claims;
using Product.Application.ServiceInterfaces;
using Product.Domain.Enum;

namespace Product.Infrastructure.Implementations.Account;

public class UserPrincipalService : IUserPrincipalService
{
    public int? UserId { get; set; }
    public UserType? UserType { get; set; }
    public int? BusinessId { get; set; }
    

    public UserPrincipalService(ClaimsPrincipal claimsPrincipal)
    {
        UserId = GetIntClaim(claimsPrincipal.Claims, "userId");
        BusinessId = GetIntClaim(claimsPrincipal.Claims, "businessId");
        UserType = GetEnumClaim<UserType>(claimsPrincipal.Claims, ClaimTypes.Role);
    }
    private string? GetClaim(IEnumerable<Claim> claims, string claimName) => claims.SingleOrDefault(c => c.Type == claimName)?.Value;
    private int? GetIntClaim(IEnumerable<Claim> claims, string claimName) => int.TryParse(GetClaim(claims, claimName), out var value) ? value : null;
    
    private TEnum? GetEnumClaim<TEnum>(IEnumerable<Claim> claims, string claimName) where TEnum : struct
    {
        var value = GetClaim(claims, claimName);
        if (Enum.TryParse<TEnum>(value, out var result))
            return result;
        return null;
    }
}
