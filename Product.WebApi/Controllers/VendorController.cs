using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class VendorController : Controller
	{
		private readonly IVendorService _vendorService;
		private readonly IVendorUserService _vendorUserService;
		private readonly IUserPrincipalService _userPrincipalService;
		public VendorController(IVendorService vendorService, IVendorUserService vendorUserService, IUserPrincipalService userPrincipalService)
		{
			_vendorService = vendorService;
			_vendorUserService = vendorUserService;
			_userPrincipalService = userPrincipalService;
		}

		[HttpPost("register/{vendorUserId}")] //Remake
		[EnsureVendorUserExists]
		[Authorize(policy: "VendorUser")]
		public async Task<IActionResult> RegisterVendor(int vendorUserId, [FromBody] VendorRegistrationDto registrationData)
		{
			var vendorUser = await _vendorUserService.GetByIdAsync(vendorUserId);

			var vendor = _vendorService.CreateVendorFromDto(vendorUser, registrationData);

			await _vendorService.CreateAsync(vendor);

			return CreatedAtAction(nameof(GetVendor), new { vendorId = vendor.Id }, vendor);
		}

		[HttpGet("Search/Operators/{operatorName}")]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<List<Operator>>> GetOperators([FromBody]string operatorName)
		{
			_vendorService.ValidateString(operatorName);

			var operators = await _vendorService.GetOperatorsByNameAsync(operatorName);

			return Ok(operators);
		}

		[HttpGet("{vendorId}")]
		[EnsureBusinessAccess(nameof(VendorUser))]
		[Authorize(policy: "VendorUser")]
		public async Task<ActionResult<Vendor>> GetVendor(int vendorId)
		{
			var vendor = await _vendorService.GetByIdAsync(vendorId);

			return Ok(vendor);
		}

		[HttpPut]
		[EnsureBusinessAccess(nameof(VendorUser))]
		[Authorize(policy: "VendorUser")]
		public async Task<IActionResult> UpdateVendor([FromBody] UpdateVendorDto vendorData)
		{
			var vendorId = _userPrincipalService.BusinessId;
			var existingVendor = await _vendorService.GetByIdAsync(vendorId!.Value);

			_vendorService.MapVendorToUpdate(existingVendor, vendorData);

			await _vendorService.UpdateAsync(existingVendor);

			return Ok(existingVendor);
		}

		[HttpDelete("{vendorId}")]
		[EnsureVendorExists]
		[Authorize(policy: "Admin")]
		public async Task<IActionResult> DeleteVendor(int vendorId)
		{
			var vendor = await _vendorService.GetByIdAsync(vendorId);

			await _vendorService.DeleteAsync(vendor);

			return NoContent();
		}
	}
}
