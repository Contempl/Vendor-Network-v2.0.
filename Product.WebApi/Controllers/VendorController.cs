using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
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

		[HttpPost("Search/Operators/")]
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
		[EnsureBusinessAccess(UserType.VendorUser)]
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
		[EnsureBusinessAccess(UserType.VendorUser)]
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
		
		[HttpPost("invite")]
		[EnsureBusinessAccess(UserType.VendorUser)]
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
