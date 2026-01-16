using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Infrastructure.Implementations;

public class VendFacilityService : IVendFacilityService
{
	private readonly IVendorFacilityRepository _vendorFacilityRepository;
	private readonly IUserPrincipalService _userPrincipalService;
	private readonly IVendorRepository _vendorRepository;
	private readonly ILogger<VendFacilityService> _logger;

	public VendFacilityService(IVendorFacilityRepository vendorFacilityRepository, 
		IUserPrincipalService userPrincipalService, 
		IVendorRepository vendorRepository,
		ILogger<VendFacilityService> logger)
	{
		_vendorFacilityRepository = vendorFacilityRepository;
		_userPrincipalService = userPrincipalService;
		_vendorRepository = vendorRepository;
		_logger = logger;
	}
	public async Task<OneOf<VendorFacility, Error>> GetFacilityWithServicesByIdAsync(int vendorFacilityId, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var vendorId = _userPrincipalService.BusinessId!.Value;
			var result = await _vendorFacilityRepository.GetFacilityWithServicesByIdAsync(vendorFacilityId, vendorId, cancellationToken);

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get facility with services.");
			return new Error();
		}
	}
	

	public async Task<OneOf<VendorFacility, NotFoundError, Error>> AddFacilityAsync(VendorFacilityDto facilityData, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			if (facilityData.Services?.Any() == false)
			{
				_logger.LogWarning("No services provided for vendor facility creation");
				return new NotFoundError();
			}
			var vendorId = _userPrincipalService.BusinessId!.Value;
			var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);

			var facility = vendor.MapVendorFacilityFromDtoToCreate(facilityData);
		
			await _vendorFacilityRepository.CreateAsync(facility, cancellationToken);

			return facility;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to create vendor facility");
			return new Error();
		}
	}

	public async Task<OneOf<VendorFacility, Error>> UpdateFacilityAsync(int facilityId, UpdateVendorFacilityDto facilityData, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var vendorId = _userPrincipalService.BusinessId!.Value;
			var facility = await _vendorFacilityRepository.GetFacilityWithServicesByIdAsync(facilityId, vendorId, cancellationToken);

			facility.MapAndUpdateVendorFacility(facilityData);
		
			await _vendorFacilityRepository.UpdateAsync(facility,cancellationToken);

			return facility;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to update vendor facility");
			return new Error();
		}
	}

	public async Task<OneOf<int, Error>> RemoveFacilityAsync(int vendorId, int facilityId, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var vendorFacility = await _vendorFacilityRepository.GetByIdAsync(vendorId, facilityId, cancellationToken);

			await _vendorFacilityRepository.DeleteAsync(vendorFacility, cancellationToken);

			return vendorFacility.Id;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to create vendor facility");
			return new Error();
		}
	}

	public async Task<OneOf<VendorFacilityService, NotFoundError, Error>> GetVendorFacilityServiceAsync(int facilityId, int facilityServiceId, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var vendorId = _userPrincipalService.BusinessId!.Value;
			var vendorFacility = await _vendorFacilityRepository.GetFacilityWithServicesByIdAsync(facilityId, vendorId, cancellationToken);
		
			if (vendorFacility?.Services == null)
			{
				_logger.LogWarning("Vendor facility has no services");
				return new NotFoundError();
			}

			var facilityService = vendorFacility.Services.FirstOrDefault(vfs => vfs.Id == facilityServiceId);
			if (facilityService == null)
			{
				_logger.LogWarning("Couldn't fetch vendor facility service");
				return new NotFoundError();
			}

			return facilityService;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch vendor facility with services");
			return new Error();
		}
	}
}
