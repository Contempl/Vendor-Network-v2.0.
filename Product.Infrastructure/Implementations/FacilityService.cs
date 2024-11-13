using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Implementations;

public class FacilityService : IFacilityService
{
	private readonly IVendorFacilityServiceRepository _facilityRepository;

	public FacilityService(IVendorFacilityServiceRepository facilityRepository)
	{
		_facilityRepository = facilityRepository;
	}

	public Task CreateAsync(VendorFacilityService vendorFacilityService)
		=> _facilityRepository.CreateAsync(vendorFacilityService);

	public async Task DeleteAsync(VendorFacilityService facilityService) => await _facilityRepository.DeleteAsync(facilityService);
	public Task<List<VendorFacilityService>> GetServicesByFacilityIdAsync(int facilityId, int vendorId) => _facilityRepository.GetServicesByFacilityIdAsync(facilityId, vendorId);
	public async Task<VendorFacilityService> GetByIdAsync(int facilityServiceId, int facilityId, int vendorId) => await _facilityRepository.GetByIdAsync(facilityServiceId, facilityId, vendorId);
	public async Task UpdateAsync(VendorFacilityService vendorFacilityService) => await UpdateAsync(vendorFacilityService);
	public VendorFacilityService MapFacilityServiceDtoToCreate(VendorFacility facility, string serviceName) =>
	new VendorFacilityService
	{
		Name = serviceName,
		VendorFacility = facility,
	};
	public void UpdateFacilityServiceName(VendorFacilityService facilityService, string serviceName)
	{
		ValidateServiceName(serviceName);
		facilityService.Name = serviceName;
	}
	public void ValidateServiceName(string serviceName)
	{
		if (string.IsNullOrEmpty(serviceName))
		{
			throw new ArgumentException($"Field '{nameof(serviceName)}' shouldn't be empty");
		}
	}
}
