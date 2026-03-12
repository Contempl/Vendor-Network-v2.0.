namespace Product.Application.Interfaces;

public interface IRepository<T>
{
    Task CreateAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    IQueryable<T> GetAll();
    Task<T?> GetByIdOrDefaultAsync(int id);
    Task DeleteAsync(T entity, CancellationToken cancellationToken);
}
