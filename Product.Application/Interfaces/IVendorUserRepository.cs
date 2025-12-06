using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IVendorUserRepository : IRepository<VendorUser>
{
	Task<VendorUser> GetByIdAsync(int vendorId, CancellationToken cancellationToken);
	
	Task<VendorUser> CreateAsync(VendorUserCreationDto dto, CancellationToken cancellationToken);
}