using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Infrastructure.Implementations;

public class FacilityService : IFacilityService
{
	private readonly IVendorFacilityServiceRepository _facilityRepository;
	private readonly IUserPrincipalService _userPrincipalService;
	private readonly IVendorFacilityRepository _vendorFacilityRepository;
	private readonly ILogger<FacilityService> _logger;
	

	public FacilityService(
		IVendorFacilityServiceRepository facilityRepository, 
		IUserPrincipalService userPrincipalService, 
		IVendorFacilityRepository vendorFacilityRepository, 
		ILogger<FacilityService> logger)
	{
		_userPrincipalService = userPrincipalService;
		_vendorFacilityRepository = vendorFacilityRepository;
		_logger = logger;
		_facilityRepository = facilityRepository;
	}
	

	public async Task<OneOf<VendorFacilityService, NotFoundError, Error>> AddFacilityServiceAsync(int facilityId, string facilityServiceName, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var vendorId = _userPrincipalService.BusinessId!.Value;
			var vendorFacility = await _vendorFacilityRepository.GetByIdAsync(vendorId, facilityId, cancellationToken);

			if (vendorFacility == null)
			{
				_logger.LogWarning("Vendor Facility not found while trying to add service.");
				return new NotFoundError("Vendor Facility not found");
			}
		
			var newFacilityService = MapFacilityServiceDtoToCreate(vendorFacility, facilityServiceName);

			await _facilityRepository.CreateAsync(newFacilityService, cancellationToken);

			return newFacilityService;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while adding facility service.");
			return new Error();
		}
	}

	public async Task<OneOf<VendorFacilityService, ValidationError, Error>> UpdateFacilityServiceAsync(int facilityId, int facilityServiceId, 
		VendorFacilityServiceDto facilityServiceDto, CancellationToken cancellationToken = default)
	{
		try
		{
			var vendorId = _userPrincipalService.BusinessId!.Value;
			var vendorFacilityService = await _facilityRepository.GetByIdAsync(vendorId, facilityId, 
				facilityServiceId, cancellationToken);

			var serviceNameIsValid = ValidateServiceName(facilityServiceDto.Name);

			if (!serviceNameIsValid)
			{
				_logger.LogWarning("Invalid Facility Service Name.");
				return new ValidationError();
			}
		
			UpdateFacilityServiceName(vendorFacilityService, facilityServiceDto.Name);

			await _facilityRepository.UpdateAsync(vendorFacilityService, cancellationToken);

			return vendorFacilityService;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while updating facility service.");
			return new Error();
		}
	}

	public async Task<OneOf<int, Error>> RemoveFacilityServiceAsync(int facilityId, int facilityServiceId, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var vendorId = _userPrincipalService.BusinessId!.Value;
			var vendorFacilityService = await _facilityRepository.GetByIdAsync(vendorId, facilityId, 
				facilityServiceId, cancellationToken);
		
			await _facilityRepository.DeleteAsync(vendorFacilityService, cancellationToken);

			return facilityServiceId;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while removing facility service.");
			return new Error();
		}
	}

	private bool ValidateServiceName(string serviceName)
	{
		if (string.IsNullOrEmpty(serviceName))
		{
			return false;
		}
		return true;
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
}
