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

	private VendorFacilityService MapFacilityServiceDtoToCreate(VendorFacility facility, string serviceName) =>
	new VendorFacilityService
	{
		Name = serviceName,
		VendorFacilityId = facility.Id,
		VendorFacility = facility,
	};

	private void UpdateFacilityServiceName(VendorFacilityService facilityService, string serviceName)
	{
		ValidateServiceName(serviceName);
		facilityService.Name = serviceName;
	}

	public async Task<Response<VendorFacilityService>> AddFacilityServiceAsync(int facilityId, string facilityServiceName, CancellationToken cancellationToken)
	{
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var vendorFacility = await _vendorFacilityRepository.GetByIdAsync(vendorId, facilityId, cancellationToken);

		if (vendorFacility == null)
		{
			return new Response<VendorFacilityService>
			{
				ErrorMessage = "Vendor Facility not found while trying to add service",
				ErrorCode = (int)ErrorCodes.InvalidVendorFacilityServiceData
			};
		}
		
		var newFacilityService = MapFacilityServiceDtoToCreate(vendorFacility, facilityServiceName);

		await _facilityRepository.CreateAsync(newFacilityService, cancellationToken);

		return new Response<VendorFacilityService>
		{
			Data = newFacilityService,
		};
	}

	public async Task<Response<VendorFacilityService>> UpdateFacilityServiceAsync(int facilityId, int facilityServiceId, 
		VendorFacilityServiceDto facilityServiceDto, CancellationToken cancellationToken)
	{
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var vendorFacilityService = await _facilityRepository.GetByIdAsync(vendorId, facilityId, facilityServiceId, cancellationToken);

		var serviceNameIsValid = ValidateServiceName(facilityServiceDto.Name);

		if (!serviceNameIsValid)
		{
			return new Response<VendorFacilityService>
			{
				ErrorMessage = "Invalid Facility Service Name",
				ErrorCode = (int)ErrorCodes.InvalidVendorFacilityServiceData,
			};
		}

		if (vendorFacilityService == null)
		{
			return new Response<VendorFacilityService>
			{
				ErrorMessage = "Vendor Facility Service not found to update",
				ErrorCode = (int)ErrorCodes.InvalidVendorFacilityServiceData,
			};
		}
		
		UpdateFacilityServiceName(vendorFacilityService, facilityServiceDto.Name);

		await _facilityRepository.UpdateAsync(vendorFacilityService, cancellationToken);

		return new Response<VendorFacilityService>
		{
			Data = vendorFacilityService,
		};
	}

	public async Task<Response<int>> RemoveFacilityServiceAsync(int facilityId, int facilityServiceId, CancellationToken cancellationToken)
	{
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var vendorFacilityService = await _facilityRepository.GetByIdAsync(vendorId, facilityId, facilityServiceId, cancellationToken);

		if (vendorFacilityService == null)
		{
			return new Response<int>
			{
				ErrorMessage = "Vendor Facility Service not found to delete",
				ErrorCode = (int)ErrorCodes.InvalidVendorFacilityServiceData,
			};
		}
		
		await _facilityRepository.DeleteAsync(vendorFacilityService, cancellationToken);

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
