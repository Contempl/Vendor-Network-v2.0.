using Product.Domain.Dto;
using Product.Domain.Entity;

namespace Product.Application.ServiceInterfaces;

public interface IJwtTokenService
{
	TokenDto GenerateToken(UserClaimDto userClaim);

	string GenerateRefreshToken();
	
	bool Validate(RefreshToken refreshToken);
}
