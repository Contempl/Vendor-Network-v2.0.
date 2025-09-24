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
	public Task DeleteAsync(User user, CancellationToken cancellationToken)
	{
		try
		{
			_users.Remove(user);
			return SaveAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			throw new Exception($"Caught exception while deleting a user in the db: {ex.Message}");
		}
	}
	public IQueryable<User> GetAll() => _users;
	public Task<User?> GetByIdOrDefaultAsync(int id) => _users.SingleOrDefaultAsync(u => u.Id == id);

	public Task<User> GetByIdAsync(int userId, CancellationToken cancellationToken)
	{
		var cacheKey = $"{CachePrefix}{userId}";
		var cached =  _redisCacheService.GetAsync<User>(cacheKey).Result;
		if (cached != null)
		{
			return Task.FromResult(cached);
		}
		
		var user = _users.FindAsync(userId, cancellationToken).Result;
		if (user == null)
			throw new KeyNotFoundException($"User with id: {userId} could not be found.");
		
		_redisCacheService.SetAsync(cacheKey, user);
		return Task.FromResult(user);
	}

	public Task UpdateAsync(User entity, CancellationToken cancellationToken)
	{
		var cacheKey = $"{CachePrefix}{entity.Id}";
		_redisCacheService.RemoveAsync(cacheKey);
		
		_users.Update(entity);
		
		_redisCacheService.SetAsync(cacheKey, entity.MapToFrontEndDto());
		
		return SaveAsync(cancellationToken);
	}

	private Task SaveAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
	public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
	{
		var user = _users.Where(u => u.Email.Trim() == email.Trim())
			.SingleOrDefaultAsync(cancellationToken).Result;
		
		if (user == null)
			return null;
		
		var cacheKey = $"{CachePrefix}{user.Id}";
		_redisCacheService.SetAsync(cacheKey, user);
		
		return Task.FromResult(user)!; 
	}
			
	public Task<User> GetByIdWithInvitesAsync(int userId, CancellationToken cancellationToken)
	{
		return _users.Include(u => u.SentInvites).FirstAsync(u => u.Id == userId, cancellationToken: cancellationToken);
	}
}
