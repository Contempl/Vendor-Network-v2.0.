using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IVendorUserRepository : IRepository<VendorUser>
{
	Task<VendorUser> GetByIdAsync(int vendorId);
}