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
		public async Task<ActionResult<Response<List<BusinessFrontEndDto>>>> GetOperators(
			[FromBody]OperatorSearchDto operatorData, CancellationToken cancellationToken)
		{
			var response = await _vendorService.SearchOperatorsAsync(operatorData, cancellationToken);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}

		[HttpGet("{vendorId}")]
		[EnsureBusinessAccess(UserType.VendorUser)]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<Vendor>> GetVendor(int vendorId, CancellationToken cancellationToken)
		{
			var response = await _vendorService.GetVendorByIdAsync(vendorId, cancellationToken);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}

		[HttpPut]
		[EnsureBusinessAccess(UserType.VendorUser)]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<Response<BusinessFrontEndDto>>> UpdateVendor(
			[FromBody] UpdateVendorDto vendorData, CancellationToken cancellationToken)
		{
			var response = await _vendorService.UpdateVendorAsync(vendorData, cancellationToken);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}
		
		[HttpPost("invite")]
		[EnsureBusinessAccess(UserType.VendorUser)]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<Response<InviteIdToFrontEnd>>> InviteVendorUser(
			[FromBody] EmailForInviteDto email, CancellationToken cancellationToken)
		{
			var response = await _vendorService.InviteVendorUserAsync(email, cancellationToken);
			if (response.IsSuccess)
			{
				return Ok(response);
			}
			return BadRequest(response);
		}
	}
}
