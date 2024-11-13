using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
	private readonly AppDbContext _context;
	private DbSet<User> _users;

	public UserRepository(AppDbContext context)
	{
		_context = context;
		_users = _context.Set<User>();
	}

	public async Task CreateAsync(User entity)
	{
		await _users.AddAsync(entity);
		await SaveAsync();
	}
	public async Task DeleteAsync(User user)
	{
		try
		{
			_users.Remove(user);
			await SaveAsync();
		}
		catch (Exception ex)
		{
			throw new Exception($"Caught exception while deleting a user in the db: {ex.Message}");
		}
	}
	public IQueryable<User> GetAll() => _users;
	public async Task<User?> GetByIdOrDefaultAsync(int id) => await _users.SingleOrDefaultAsync(u => u.Id == id);
	public async Task<User> GetByIdAsync(int id) => await _users.SingleAsync(u => u.Id == id);
	public async Task UpdateAsync(User entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException(nameof(entity));
		}

		_users.Update(entity);
		await SaveAsync();
	}
	public async Task SaveAsync() => await _context.SaveChangesAsync();
	public Task<User> GetByEmailAsync(string email)
	{
		var user = _users.Where(u => u.Email == email).SingleAsync();
		return user;
	}
}
