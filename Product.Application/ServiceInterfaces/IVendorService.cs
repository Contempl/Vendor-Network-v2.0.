using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Application.ServiceInterfaces;

public interface IVendorService
{
    Task CreateAsync(Vendor vendor);
    Task<Vendor> GetByIdAsync(int id);
    Task UpdateAsync(Vendor vendor);
    Task DeleteAsync(Vendor vendor);
    Vendor CreateVendorFromDto(VendorUser user, VendorRegistrationDto registrationData);
    void ValidateString(string input);
    void MapVendorToUpdate(Vendor vendor, UpdateVendorDto vendorData);
	Task<List<Operator>> GetOperatorsByNameAsync(string operatorName);
}
