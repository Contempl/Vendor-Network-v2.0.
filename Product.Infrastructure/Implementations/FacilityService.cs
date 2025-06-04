using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;

namespace Product.Infrastructure.Implementations;

public class FacilityService : IFacilityService
{
	private readonly IVendorFacilityServiceRepository _facilityRepository;
	private readonly IUserPrincipalService _userPrincipalService;
	private readonly IVendorFacilityRepository _vendorFacilityRepository;
	

	public FacilityService(IVendorFacilityServiceRepository facilityRepository, IUserPrincipalService userPrincipalService, IVendorFacilityRepository vendorFacilityRepository)
	{
		_userPrincipalService = userPrincipalService;
		_vendorFacilityRepository = vendorFacilityRepository;
		_facilityRepository = facilityRepository;
	}

	public Task CreateAsync(VendorFacilityService vendorFacilityService)
		=> _facilityRepository.CreateAsync(vendorFacilityService);

	public async Task DeleteAsync(VendorFacilityService facilityService) => await _facilityRepository.DeleteAsync(facilityService);
	public Task<List<VendorFacilityService>> GetServicesByFacilityIdAsync(int facilityId, int vendorId) => _facilityRepository.GetServicesByFacilityIdAsync(facilityId, vendorId);
	public async Task<VendorFacilityService> GetByIdAsync(int facilityServiceId, int facilityId, int vendorId) => await _facilityRepository.GetByIdAsync(facilityServiceId, facilityId, vendorId);
	public async Task UpdateAsync(VendorFacilityService vendorFacilityService) => await _facilityRepository.UpdateAsync(vendorFacilityService);
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

	public async Task<Response<VendorFacilityService>> AddFacilityServiceAsync(int facilityId, string facilityServiceName)
	{
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var facility = await _vendorFacilityRepository.GetByIdAsync(facilityId, vendorId);

		var newFacilityService = MapFacilityServiceDtoToCreate(facility, facilityServiceName);

		await _facilityRepository.CreateAsync(newFacilityService);

		return new Response<VendorFacilityService>
		{
			Data = newFacilityService,
		};
	}

	public async Task<Response<VendorFacilityService>> UpdateFacilityServiceAsync(int facilityId, int facilityServiceId, VendorFacilityServiceDto facilityServiceDto)
	{
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var vendorFacilityService = await _facilityRepository.GetByIdAsync(vendorId, facilityId, facilityServiceId);

		var serviceNameIsValid = ValidateServiceName(facilityServiceDto.Name);

		if (!serviceNameIsValid)
		{
			return new Response<VendorFacilityService>
			{
				ErrorMessage = "Invalid Facility Service Name",
				ErrorCode = (int)ErrorCodes.InvalidVendorFacilityServiceData,
			};
		}

		UpdateFacilityServiceName(vendorFacilityService, facilityServiceDto.Name);

		await _facilityRepository.UpdateAsync(vendorFacilityService);

		return new Response<VendorFacilityService>
		{
			Data = vendorFacilityService,
		};
	}

	public async Task<Response<int>> RemoveFacilityServiceAsync(int facilityId, int facilityServiceId)
	{
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var facilityService = await _facilityRepository.GetByIdAsync(vendorId, facilityId, facilityServiceId);

		await _facilityRepository.DeleteAsync(facilityService);

		var result = new Response<int>
		{
			Data = facilityServiceId,
		};
		return result;
	}

	private bool ValidateServiceName(string serviceName)
	{
		if (string.IsNullOrEmpty(serviceName))
		{
			return false;
		}
		return true;
	}
}
