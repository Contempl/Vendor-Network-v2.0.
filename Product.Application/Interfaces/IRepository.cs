namespace Product.Application.Interfaces;

public interface IRepository<T>
{
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    IQueryable<T> GetAll();
    Task<T?> GetByIdOrDefaultAsync(int id);
    Task DeleteAsync(T entity);
    Task SaveAsync();
}
