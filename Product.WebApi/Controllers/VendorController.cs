using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class VendorController : Controller
	{
		private readonly IVendorService _vendorService;

		public VendorController(IVendorService vendorService)
		{
			_vendorService = vendorService;
		}

		[HttpPost("register/{vendorUserId}")] //Remove
		[EnsureVendorUserExists]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<Response<BusinessFrontEndDto>>> RegisterVendor(int vendorUserId, [FromBody] VendorRegistrationDto registrationData)
		{
			var response = await _vendorService.RegisterVendorAsync(vendorUserId, registrationData);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}

		[HttpGet("Search/Operators/{operatorName}")]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<Response<List<BusinessFrontEndDto>>>> GetOperators([FromBody]OperatorSearchDto operatorData)
		{
			var response = await _vendorService.SearchOperatorsAsync(operatorData);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}

		[HttpGet("{vendorId}")]
		[EnsureBusinessAccess(nameof(VendorUser))]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<Vendor>> GetVendor(int vendorId)
		{
			var response = await _vendorService.GetVendorByIdAsync(vendorId);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}

		[HttpPut]
		[EnsureBusinessAccess(nameof(VendorUser))]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<Response<BusinessFrontEndDto>>> UpdateVendor([FromBody] UpdateVendorDto vendorData)
		{
			var response = await _vendorService.UpdateVendorAsync(vendorData);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}

		[HttpDelete("{vendorId}")]
		[EnsureVendorExists]
		[Authorize(policy: "AdminOnly")]
		public async Task<ActionResult<Response<int>>> DeleteVendor(int vendorId)
		{
			var response = await _vendorService.DeleteVendorAsync(vendorId);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}
		
		[HttpPost("invite")]
		[EnsureBusinessAccess(nameof(VendorUser))]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<Response<InviteIdToFrontEnd>>> InviteVendorUser([FromBody] EmailForInviteDto email)
		{
			var response = await _vendorService.InviteVendorUserAsync(email);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}
	}
}
