using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Application.ServiceInterfaces;

public interface IAdministratorService
{
    Task CreateAsync(Administrator admin);
    Task<Administrator> GetByIdAsync(int adminId);
    Task UpdateAsync(Administrator admin);
    Task DeleteAsync(Administrator admmin);
    string GenerateEmailTemplate(string email, User user, string inviteUrl);
    Administrator MapAdminFromDto(AdminRegistrationDto registrationDto);
}
