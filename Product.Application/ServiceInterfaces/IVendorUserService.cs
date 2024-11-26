using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.ServiceInterfaces;

public interface IVendorUserService
{
    Task CreateAsync(VendorUser vendorUser);
    Task<VendorUser> GetByIdAsync(int vendorUserId);
    Task UpdateAsync(VendorUser vendorUser);
    Task DeleteAsync(VendorUser vendorUser);
    VendorUser MapVendorUserFromDto(UserRegistrationDto registrationData);
}
