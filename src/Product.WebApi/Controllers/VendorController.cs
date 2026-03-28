using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
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
		public async Task<ActionResult<OneOf<List<BusinessFrontEndDto>, InvalidOperatorNameError, Error>>> GetOperators(
			[FromBody]OperatorSearchDto operatorData, CancellationToken cancellationToken)
		{
			var response = await _vendorService.SearchOperatorsAsync(operatorData, cancellationToken);

			return response.Match<ActionResult>(
				operators => Ok(operators),
				invalid => BadRequest(invalid),
				error => StatusCode(500, error));
		}

		[HttpGet("{vendorId}")]
		[EnsureBusinessAccess(UserType.VendorUser)]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<OneOf<BusinessFrontEndDto, Error>>> GetVendor(int vendorId, CancellationToken cancellationToken)
		{
			var response = await _vendorService.GetVendorByIdAsync(vendorId, cancellationToken);

			return response.Match<ActionResult>(
				dto => Ok(dto),
				error => StatusCode(500, error));
		}

		[HttpPut]
		[EnsureBusinessAccess(UserType.VendorUser)]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<OneOf<BusinessFrontEndDto, Error>>> UpdateVendor(
			[FromBody] UpdateVendorDto vendorData, CancellationToken cancellationToken)
		{
			var response = await _vendorService.UpdateVendorAsync(vendorData, cancellationToken);
			
			return response.Match<ActionResult>(
				dto => Ok(dto),
				error => StatusCode(500, error));
		}
		
		[HttpPost("invite")]
		[EnsureBusinessAccess(UserType.VendorUser)]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<OneOf<MailMsg, Error>>> InviteVendorUser(
			[FromBody] EmailForInviteDto email, CancellationToken cancellationToken)
		{
			var response = await _vendorService.InviteVendorUserAsync(email, cancellationToken);
			
			return response.Match<ActionResult>(
				mailMsg => Ok(mailMsg),
				error => StatusCode(500, error));
		}

		[HttpGet("facilities")]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<OneOf<IEnumerable<VendorFacilityDto>, Error>>> GetVendorFacilities(
			CancellationToken cancellationToken)
		{
			var response = await _vendorService.GetVendorFacilitiesAsync(cancellationToken);
			
			return response.Match<ActionResult>(
				facilities => Ok(facilities),
				error => BadRequest(error));
		}
	}
}
