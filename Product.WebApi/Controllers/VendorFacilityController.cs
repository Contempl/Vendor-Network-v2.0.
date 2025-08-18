using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("vendor")]
[ApiController]
public class VendorFacilityController : ControllerBase
{
	private readonly IVendFacilityService _vendorFacilityService;
	private readonly IFacilityService _facilityService;

	public VendorFacilityController(IVendFacilityService vendorFacilityService, IFacilityService facilityService)
	{
		_vendorFacilityService = vendorFacilityService;
		_facilityService = facilityService;
	}

	[HttpGet("/facility/{facilityId}")]
	[EnsureVendorFacilityExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<Response<VendorFacility>>> GetVendorFacility(int facilityId)
	{
		var response = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("/facility")]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<Response<VendorFacility>>> AddFacility(VendorFacilityDto facilityData)
	{
		var response = await _vendorFacilityService.AddFacilityAsync(facilityData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPut("facility/{facilityId}")]
	[EnsureVendorFacilityExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<Response<VendorFacility>>> UpdateFacility(int facilityId,
		[FromBody] UpdateVendorFacilityDto facilityData)
	{
		var response = await _vendorFacilityService.UpdateFacilityAsync(facilityId, facilityData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpDelete("{vendorId}/facilities/{facilityId}")]
	[EnsureVendorFacilityExists] 
	[Authorize(policy: "VendorUser")] 
	public async Task<ActionResult<Response<int>>> DeleteFacility(int vendorId, int facilityId)
	{
		var response = await _vendorFacilityService.RemoveFacilityAsync(vendorId, facilityId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}


	[HttpGet("/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<Response<VendorFacilityService>>> GetVendorFacilityService(int facilityId, int facilityServiceId)
	{
		var response = await _vendorFacilityService.GetVendorFacilityServiceAsync(facilityId, facilityServiceId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
		
	}

	[HttpGet("/facility/{facilityId}/services")]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<List<VendorFacilityService>>> GetVendorFacilityServices(int facilityId)
	{
		var response = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("/facility/{facilityId}/service")]
	[EnsureVendorFacilityExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<Response<VendorFacilityService>>> AddFacilityService(int facilityId, [FromBody] string facilityServiceName)
	{
		var response = await _facilityService.AddFacilityServiceAsync(facilityId, facilityServiceName);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPut("/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureVendorFacilityServiceExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<Response<VendorFacilityService>>> UpdateFacilityService(int facilityId, int facilityServiceId, [FromBody] VendorFacilityServiceDto facilityServiceDto)
	{
		var response = await _facilityService.UpdateFacilityServiceAsync(facilityId, facilityServiceId, facilityServiceDto);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpDelete("/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureVendorFacilityServiceExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<Response<VendorFacilityService>>> DeleteFacilityService(int facilityId, int facilityServiceId)
	{
		var response = await _facilityService.RemoveFacilityServiceAsync(facilityId, facilityServiceId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
}
