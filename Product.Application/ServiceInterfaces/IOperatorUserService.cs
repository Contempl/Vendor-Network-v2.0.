using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorUserService
{
    Task CreateAsync(OperatorUser operatorUser);
    Task<OperatorUser> GetByIdAsync(int operatorServiceId);
    Task UpdateAsync(OperatorUser operatorUser);
    Task DeleteAsync(OperatorUser operatorUser);
    OperatorUser MapOperatorUserFromDto(UserRegistrationDto registrationData);
}
