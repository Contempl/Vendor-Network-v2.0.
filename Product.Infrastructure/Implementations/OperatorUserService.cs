using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Implementations;

public class OperatorUserService : IOperatorUserService
{
    private readonly IOperatorUserRepository _operatorUserRepository;
	private readonly IPasswordHasher _passwordHasher;

	public OperatorUserService(IOperatorUserRepository operatorUserRepository, IPasswordHasher userPrincipalService)
	{
		_operatorUserRepository = operatorUserRepository;
		_passwordHasher = userPrincipalService;
	}

	public Task CreateAsync(OperatorUser operatorUser) => _operatorUserRepository.CreateAsync(operatorUser);
    public Task DeleteAsync(OperatorUser operatorUser) => _operatorUserRepository.DeleteAsync(operatorUser);
    public Task<OperatorUser> GetByIdAsync(int operatorUserId) => _operatorUserRepository.GetByIdAsync(operatorUserId);
    public Task UpdateAsync(OperatorUser vendor) => _operatorUserRepository.UpdateAsync(vendor);
    public OperatorUser MapOperatorUserFromDto(UserRegistrationDto registrationData) => new OperatorUser
    {
        UserName = registrationData.UserName,
        FirstName = registrationData.FirstName,
        LastName = registrationData.LastName,
        Email = registrationData.Email,
        PasswordHash = _passwordHasher.HashThePassword(registrationData.Password)
    };
}
