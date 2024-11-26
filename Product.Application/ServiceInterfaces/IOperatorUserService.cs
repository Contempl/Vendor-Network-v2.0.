using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorUserService
{
    Task CreateAsync(OperatorUser operatorUser);
    Task<OperatorUser> GetByIdAsync(int operatorUserId);
    Task UpdateAsync(OperatorUser operatorUser);
    Task DeleteAsync(OperatorUser operatorUser);
    OperatorUser MapOperatorUserFromDto(UserRegistrationDto registrationData);
}
