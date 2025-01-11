using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.ServiceInterfaces;

public interface IAdministratorService
{
    Task CreateAsync(Administrator admin);
    Task<Administrator> GetByIdAsync(int adminId);
    Task UpdateAsync(Administrator admin);
    Task DeleteAsync(Administrator admmin);
    Administrator MapAdminFromDto(AdminRegistrationDto registrationDto);
}
