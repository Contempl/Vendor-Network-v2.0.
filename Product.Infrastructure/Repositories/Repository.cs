using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;

namespace Product.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _table;

    public Repository(AppDbContext context)
    {
        _context = context;
        _table = _context.Set<T>();
    }
	public IQueryable<T> GetAll() => _table;
    public async Task CreateAsync(T entity, CancellationToken cancellationToken)
    {
        await _table.AddAsync(entity, cancellationToken);
        await SaveAsync(cancellationToken);
	}
    public Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        _context.Remove(entity);
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<T?> GetByIdOrDefaultAsync(int id)
    {
        var entity = await _table.FindAsync(id);
        return entity;
    }
    
    private Task SaveAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
    public Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
		_table.Update(entity);
        return SaveAsync(cancellationToken);
    }
}
