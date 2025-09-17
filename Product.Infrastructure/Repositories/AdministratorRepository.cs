using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class AdministratorRepository : IAdministratorRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<Administrator> _administrators;
	private readonly IRedisCacheService _redisCacheService;
	private const string CachePrefix = "User_";
	public AdministratorRepository(AppDbContext context, IRedisCacheService redisCacheService)
	{
		_context = context;
		_redisCacheService = redisCacheService;
		_administrators = _context.Administrators;
	}
	public async Task CreateAsync(Administrator admin, CancellationToken cancellationToken = default)
	{
		await _administrators.AddAsync(admin, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public async Task DeleteAsync(Administrator admin, CancellationToken cancellationToken = default)
	{
		_administrators.Remove(admin);
		await SaveAsync(cancellationToken);
	}
	public IQueryable<Administrator> GetAll() => _administrators;
	public async Task<Administrator?> GetByIdOrDefaultAsync(int adminId) => await _administrators.SingleOrDefaultAsync(admin => admin.Id == adminId);
	public async Task<Administrator> GetByIdAsync(int adminId, CancellationToken cancellationToken = default) => await _administrators.SingleAsync(admin => admin.Id == adminId, cancellationToken);
	public async Task<Administrator?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		var admin = await _administrators.Where(u => u.Email.Trim() == email.Trim())
			.SingleOrDefaultAsync(cancellationToken);
		if (admin == null)
			return null;
		
		var cacheKey = $"{CachePrefix}{admin.Id}";
		await _redisCacheService.SetAsync(cacheKey, admin);
		
		return admin; 
	}

	private Task SaveAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
	public async Task UpdateAsync(Administrator admin, CancellationToken cancellationToken = default)
	{
		_administrators.Update(admin);
		await SaveAsync(cancellationToken);
	}
}
