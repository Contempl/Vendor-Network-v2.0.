using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IAdministratorRepository : IRepository<Administrator>
{
    Task<Administrator> GetByIdAsync(int adminId);
}