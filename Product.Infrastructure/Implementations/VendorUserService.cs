using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Implementations;

public class VendorUserService : IVendorUserService
{
    private readonly IVendorUserRepository _vendorUserRepository;
	private readonly IPasswordHasher _passwordHasher;

	public VendorUserService(IVendorUserRepository vendorUserRepository, IPasswordHasher userPrincipalService)
	{
		_vendorUserRepository = vendorUserRepository;
		_passwordHasher = userPrincipalService;
	}

	public Task CreateAsync(VendorUser vendor) => _vendorUserRepository.CreateAsync(vendor);
    public Task DeleteAsync(VendorUser vendorUser) => _vendorUserRepository.DeleteAsync(vendorUser);
    public Task<VendorUser> GetByIdAsync(int id) => _vendorUserRepository.GetByIdAsync(id);
    public Task UpdateAsync(VendorUser vendor) => _vendorUserRepository.UpdateAsync(vendor);
    public VendorUser MapVendorUserFromDto(UserRegistrationDto registrationData) => new VendorUser()
    {
        UserName = registrationData.UserName,
        FirstName = registrationData.FirstName,
        LastName = registrationData.LastName,
        Email = registrationData.Email,
        PasswordHash = _passwordHasher.HashThePassword(registrationData.Password),
    };

}
