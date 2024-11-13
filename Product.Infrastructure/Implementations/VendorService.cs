using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Infrastructure.Implementations;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IOperatorRepository _operatorRepository;

    public VendorService(IVendorRepository vendorRepository, IOperatorRepository operatorRepository)
    {
        _vendorRepository = vendorRepository;
        _operatorRepository = operatorRepository;
    }

    public Task CreateAsync(Vendor vendor) => _vendorRepository.CreateAsync(vendor);
    public Task DeleteAsync(Vendor vendor) => _vendorRepository.DeleteAsync(vendor);
    public Task<Vendor> GetByIdAsync(int id) => _vendorRepository.GetByIdAsync(id);
    public async Task UpdateAsync(Vendor vendor)
    {
        try
        {
            await _vendorRepository.UpdateAsync(vendor);
        }
        catch (Exception e)    
        {
            throw new Exception($"Failed to update a vendor: {e.Message}");
        }
        await Task.CompletedTask;
    }
	public Vendor CreateVendorFromDto(VendorUser user, 
        VendorRegistrationDto registrationData) => new Vendor
    {
		BusinessName = registrationData.BusinessName,
		Adress = registrationData.Adress,
		Email = registrationData.Email,
		VendorUsers = new List<VendorUser> { user }
	};
    public void ValidateString(string input)
    {
		if (string.IsNullOrWhiteSpace(input))
		{
            throw new ArgumentException($"stirng '{nameof(input)}' shouldn't be empty");
		}
	}
    public void MapVendorToUpdate(Vendor vendor, UpdateVendorDto vendorData)
    {
		vendor.BusinessName = vendorData.BusinessName ?? vendor.BusinessName;
		vendor.Adress = vendorData.Address ?? vendor.Adress;
		vendor.Email = vendorData.Email ??  vendor.Email;
	}
    public async Task<List<Operator>> GetOperatorsByNameAsync(string operatorName)
    {
        if (string.IsNullOrWhiteSpace(operatorName))
        {
            return new List<Operator>();
        }

		return await _operatorRepository.GetOperatorsByNameAsync(operatorName);
	}
}
