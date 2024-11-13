using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;

namespace Product.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private DbSet<T> _table;

    public Repository(AppDbContext context)
    {
        _context = context;
        _table = _context.Set<T>();
    }
	public IQueryable<T> GetAll() => _table;
    public async Task CreateAsync(T entity)
    {
        await _table.AddAsync(entity);
        await SaveAsync();
	}
    public async Task DeleteAsync(T entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }
    public async Task<List<T>> GetAllAsync() => await _table.ToListAsync();
    public async Task<T?> GetByIdOrDefaultAsync(int id) => await _table.FindAsync(id);
    public async Task SaveAsync() => await _context.SaveChangesAsync();
    public async Task UpdateAsync(T entity)
    {
		_table.Update(entity);
        await SaveAsync();
    }
}
