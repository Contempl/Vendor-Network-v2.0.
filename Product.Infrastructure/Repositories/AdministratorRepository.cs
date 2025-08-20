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
	public async Task CreateAsync(Administrator admin)
	{
		await _administrators.AddAsync(admin);
		await SaveAsync();
	}
	public async Task DeleteAsync(Administrator admin)
	{
		_administrators.Remove(admin);
		await SaveAsync();
	}
	public IQueryable<Administrator> GetAll() => _administrators;
	public async Task<Administrator?> GetByIdOrDefaultAsync(int AdminId) => await _administrators.SingleOrDefaultAsync(admin => admin.Id == AdminId);
	public async Task<Administrator> GetByIdAsync(int AdminId) => await _administrators.SingleAsync(admin => admin.Id == AdminId);
	public async Task<Administrator?> GetByEmailAsync(string email)
	{
		var admin = await _administrators.Where(u => u.Email.Trim() == email.Trim())
			.SingleOrDefaultAsync();
		if (admin == null)
			return null;
		
		var cacheKey = $"{CachePrefix}{admin.Id}";
		await _redisCacheService.SetAsync(cacheKey, admin);
		
		return admin; 
	}

	private Task SaveAsync() => _context.SaveChangesAsync();
	public async Task UpdateAsync(Administrator admin)
	{
		_administrators.Update(admin);
		await SaveAsync();
	}
}
