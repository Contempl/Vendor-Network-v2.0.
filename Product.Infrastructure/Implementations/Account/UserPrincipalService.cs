using System.Security.Claims;
using Product.Application.ServiceInterfaces;

namespace Product.Infrastructure.Implementations.Account;

public class UserPrincipalService : IUserPrincipalService
{
    public int? UserId { get; set; }
    public string? UserType { get; set; } //enum
    public int BusinessId { get; set; }
    

    public UserPrincipalService(ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.Claims.SingleOrDefault(c => c.Type == "userId");
        if (claim != null)
        {
            var userId = int.Parse(claim.Value);
            UserId = userId;
        }
    }
}
