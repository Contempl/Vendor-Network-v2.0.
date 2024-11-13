using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;
using Product.Infrastructure.FIlters;

namespace Product.WebApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class VendorController : Controller
	{
		private readonly IVendorService _vendorService;
		private readonly IVendorUserService _vendorUserService;
		public VendorController(IVendorService vendorService, IVendorUserService vendorUserService)
		{
			_vendorService = vendorService;
			_vendorUserService = vendorUserService;
		}

		[HttpPost("register/{vendorUserId}")]
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
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<List<Operator>>> GetOperators(string operatorName)
		{
			_vendorService.ValidateString(operatorName);

			var operators = await _vendorService.GetOperatorsByNameAsync(operatorName);

			return Ok(operators);
		}

		[HttpGet("{vendorId}")]
		[EnsureVendorExists]
		[Authorize]
		public async Task<ActionResult<Vendor>> GetVendor(int vendorId)
		{
			var vendor = await _vendorService.GetByIdAsync(vendorId);

			return Ok(vendor);
		}

		[HttpPut("{vendorId}")]
		[EnsureVendorExists]
		[Authorize]
		public async Task<IActionResult> UpdateVendor(int vendorId, [FromBody] UpdateVendorDto vendorData)
		{
			var existingVendor = await _vendorService.GetByIdAsync(vendorId);

			_vendorService.MapVendorToUpdate(existingVendor, vendorData);

			await _vendorService.UpdateAsync(existingVendor);

			return Ok(existingVendor);
		}

		[HttpDelete("{vendorId}")]
		[EnsureVendorExists]
		[Authorize]
		public async Task<IActionResult> DeleteVendor(int vendorId)
		{
			var vendor = await _vendorService.GetByIdAsync(vendorId);

			await _vendorService.DeleteAsync(vendor);

			return NoContent();
		}
	}
}
