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
	public async Task CreateAsync(Administrator admin, CancellationToken cancellationToken)
	{
		await _administrators.AddAsync(admin, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public Task DeleteAsync(Administrator admin, CancellationToken cancellationToken)
	{
		_administrators.Remove(admin);
		return SaveAsync(cancellationToken);
	}
	public IQueryable<Administrator> GetAll() => _administrators;
	public Task<Administrator?> GetByIdOrDefaultAsync(int adminId) =>  _administrators.SingleOrDefaultAsync(admin => admin.Id == adminId);
	public Task<Administrator> GetByIdAsync(int adminId, CancellationToken cancellationToken) =>  _administrators.SingleAsync(admin => admin.Id == adminId, cancellationToken);
	public Task<Administrator?> GetByEmailAsync(string email, CancellationToken cancellationToken)
	{
		var admin =  _administrators.Where(u => u.Email.Trim() == email.Trim())
			.SingleOrDefaultAsync(cancellationToken);
		if (admin == null)
			return null;
		
		var cacheKey = $"{CachePrefix}{admin.Id}";
		_redisCacheService.SetAsync(cacheKey, admin);
		
		return admin; 
	}

	private Task SaveAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
	public Task UpdateAsync(Administrator admin, CancellationToken cancellationToken)
	{
		_administrators.Update(admin);
		return SaveAsync(cancellationToken);
	}
}
