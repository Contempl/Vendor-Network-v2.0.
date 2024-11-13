using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;
using Product.Infrastructure.FIlters;

namespace Product.WebApi.Controllers;

[Route("vendor")]
[ApiController]
public class VendorFacilityController : ControllerBase
{
	private readonly IVendorService _vendorService;
	private readonly IVendFacilityService _vendorFacilityService;
	private readonly IFacilityService _facilityService;

	public VendorFacilityController(IVendFacilityService vendorFacilityService,
		IFacilityService facilityService, IVendorService vendorService)
	{
		_vendorFacilityService = vendorFacilityService;
		_facilityService = facilityService;
		_vendorService = vendorService;
	}

	[HttpGet("{vendorId}/facility/{facilityId}")]
	[EnsureVendorFacilityExists]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<VendorFacility>> GetVendorFacility(int vendorId, int facilityId)
	{
		var vendorFaciltiy = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId, vendorId);

		return Ok(vendorFaciltiy);
	}

	[HttpPost("{vendorId}/facility")]
	[EnsureVendorExists]
	[Authorize(policy: "VendorUser")]
	public async Task<IActionResult> AddFacility(int vendorId, VendorFacilityDto facilityData)
	{
		var vendor = await _vendorService.GetByIdAsync(vendorId);

		var facility = _vendorFacilityService.MapVendorFacilityFromDtoToCreateAsync(vendor, facilityData);

		await _vendorFacilityService.CreateAsync(facility);

		return CreatedAtAction(nameof(GetVendorFacility), new { vendorId = vendor.Id, facilityId = facility.Id }, facility);
	}

	[HttpPut("facility/{facilityId}")]
	[EnsureVendorExists]
	[EnsureVendorFacilityExists]
	[Authorize(policy: "VendorUser")]
	public async Task<IActionResult> UpdateFacility(int vendorId, int facilityId,
		[FromBody] UpdateVendorFacilityDto facilityData)
	{
		var facility = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId, vendorId);

		await _vendorFacilityService.MapAndUpdateVendorFacility(facility, facilityData);
		return Ok(facility);
	}

	[HttpDelete("facilities/{facilityId}")]
	[EnsureVendorFacilityExists]
	[Authorize(policy: "AdminOnly")]
	public async Task<ActionResult> DeleteFacility(int vendorId, int facilityId)
	{
		var vendorFacility = await _vendorFacilityService.GetByIdAsync(vendorId, facilityId);

		await _vendorFacilityService.DeleteAsync(vendorFacility);

		return NoContent();
	}


	[HttpGet("{vendorId}/facility/{facilityId}/service/{facilityServiceId}")]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult> GetVendorFacilityService(int vendorId, int facilityId, int facilityServiceId)
	{
		var vendorFacility = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId, vendorId);

		return vendorFacility switch
		{
			null => BadRequest("Invalid data"),
			{ VendorId: var vendId } when vendId != vendorId => BadRequest("Data access prohibited"),
			{ Services: var services } => services.FirstOrDefault(vfs => vfs.Id == facilityServiceId) switch
			{
				null => NotFound($"Service not found with id: {facilityServiceId}"),
				var facilityService => Ok(facilityService)
			},
		};
	}

	[HttpGet("{vendorId}/facility/{facilityId}/services")]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<List<VendorFacilityService>>> GetVendorFacilityServices(int vendorId, int facilityId)
	{
		var facilityServices = await _facilityService.GetServicesByFacilityIdAsync(vendorId, facilityId);

		return Ok(facilityServices);
	}

	[HttpPost("{vendorId}/facility/{facilityId}/service")]
	[EnsureVendorFacilityExists]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult> AddFacilityService(int vendorId, int facilityId, string facilityServiceName)
	{
		var facility = await _vendorFacilityService.GetByIdAsync(facilityId, vendorId);

		var newFacilityService = _facilityService.MapFacilityServiceDtoToCreate(facility, facilityServiceName);

		await _facilityService.CreateAsync(newFacilityService);

		return Ok(newFacilityService);
	}

	[HttpPut("{vendorId}/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureVendorFacilityServiceExists]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult> UpdateFacilityService(int vendorId, int facilityId, int facilityServiceId, string facilityServiceName)
	{
		var vendorFacilityService = await _facilityService.GetByIdAsync(vendorId, facilityId, facilityServiceId);

		_facilityService.ValidateServiceName(facilityServiceName);

		_facilityService.UpdateFacilityServiceName(vendorFacilityService, facilityServiceName);

		await _facilityService.UpdateAsync(vendorFacilityService);

		return Ok(vendorFacilityService);
	}

	[HttpDelete("{vendorId}/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureVendorFacilityServiceExists]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult> DeleteFacilityService(int vendorId, int facilityId, int facilityServiceId)
	{
		var facilityService = await _facilityService.GetByIdAsync(vendorId, facilityId, facilityServiceId);

		await _facilityService.DeleteAsync(facilityService);

		return NoContent();
	}
}
