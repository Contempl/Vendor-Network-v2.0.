using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IRefreshTokenRepository 
{
    Task CreateAsync(RefreshToken token, CancellationToken cancellationToken);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken);
}