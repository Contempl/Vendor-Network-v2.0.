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
}
