using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<User> _users;
	private readonly IRedisCacheService _redisCacheService;
	private const string CachePrefix = "User_";
	public UserRepository(AppDbContext context, IRedisCacheService redisCacheService)
	{
		_context = context;
		_redisCacheService = redisCacheService;
		_users = _context.Set<User>();
	}

	public async Task CreateAsync(User entity, CancellationToken cancellationToken)
	{
		await _users.AddAsync(entity, cancellationToken);
		await SaveAsync(cancellationToken);
		
		await _redisCacheService.SetAsync(CachePrefix + entity.Id, entity);
	}
	public async Task DeleteAsync(User user, CancellationToken cancellationToken)
	{
		try
		{
			_users.Remove(user);
			await SaveAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			throw new Exception($"Caught exception while deleting a user in the db: {ex.Message}");
		}
	}
	public IQueryable<User> GetAll() => _users;
	public async Task<User?> GetByIdOrDefaultAsync(int id) => await _users.SingleOrDefaultAsync(u => u.Id == id);

	public async Task<User> GetByIdAsync(int userId, CancellationToken cancellationToken)
	{
		var cacheKey = $"{CachePrefix}{userId}";
		var cached = await _redisCacheService.GetAsync<User>(cacheKey);
		if (cached != null)
		{
			return cached;
		}
		
		var user = await _users.FindAsync(userId, cancellationToken);
		if (user == null)
			throw new KeyNotFoundException($"User with id: {userId} could not be found.");
		
		await _redisCacheService.SetAsync(cacheKey, user);
		return user;
	}

	public async Task UpdateAsync(User entity, CancellationToken cancellationToken)
	{
		var cacheKey = $"{CachePrefix}{entity.Id}";
		await _redisCacheService.RemoveAsync(cacheKey);
		
		_users.Update(entity);
		await SaveAsync(cancellationToken);
		
		await _redisCacheService.SetAsync(cacheKey, entity.MapToFrontEndDto());
	}

	private async Task SaveAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
	public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
	{
		var user = await _users.Where(u => u.Email.Trim() == email.Trim())
			.SingleOrDefaultAsync(cancellationToken);
		if (user == null)
			return null;
		
		var cacheKey = $"{CachePrefix}{user.Id}";
		await _redisCacheService.SetAsync(cacheKey, user);
		
		return user; 
	}
			
	public async Task<User> GetByIdWithInvitesAsync(int userId, CancellationToken cancellationToken)
	{
		return await _users.Include(u => u.SentInvites).FirstAsync(u => u.Id == userId, cancellationToken: cancellationToken);
	}
}
