using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<RefreshToken> _refreshTokens;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
        _refreshTokens = _context.RefreshTokens;
    }

    public async Task CreateAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        await _refreshTokens.AddAsync(entity, cancellationToken);
        await SaveAsync(cancellationToken);
    }
    private Task SaveAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);


    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) => 
        await _refreshTokens.SingleOrDefaultAsync(r => r.Token == token, cancellationToken);

    public Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        token.Revoked = true;
        return SaveAsync(cancellationToken);
    }
}