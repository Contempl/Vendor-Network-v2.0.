using Product.Domain.Entity;

namespace Product.Application.ServiceInterfaces;

public interface IJwtTokenService
{
	string GenerateToken(User user);
}
