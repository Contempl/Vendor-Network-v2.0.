using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Result;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(policy: "Admin")]
public class AdminController : ControllerBase
{
	private readonly IAdministratorService _adminService;
	private readonly IUserPrincipalService _userPrincipalService;

	public AdminController(IAdministratorService adminService, IUserPrincipalService userPrincipalService)
	{
		_adminService = adminService;
		_userPrincipalService = userPrincipalService;
	}

	[HttpPost("inviteBusiness")]
	[EnsureAdministratorExists]
	public async Task<ActionResult<Response<InviteIdToFrontEnd>>> InviteBusiness([FromBody] BusinessInvitationData invitationData)
	{
		var adminId = _userPrincipalService.UserId!.Value;
		var response = await _adminService.InviteBusiness(adminId, invitationData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("/inviteVendorUser")]
	[EnsureAdministratorExists]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> InviteVendorUser([FromBody] DataForInviteDto inviteData)
	{
		var adminId = _userPrincipalService.UserId!.Value;
		var response = await _adminService.InviteVendorUser(adminId, inviteData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("/inviteOperatorUser")]
	[EnsureAdministratorExists]
	public async Task<IActionResult> InviteOperatorUser([FromBody] DataForInviteDto inviteData)
	{
		var adminId = _userPrincipalService.UserId!.Value;
		var response = await _adminService.InviteOperatorUser(adminId, inviteData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
	
	
	// [HttpDelete("{vendorId}")]
	// [EnsureVendorExists]
	// [Authorize(policy: "Admin")]
	// public async Task<ActionResult<Response<int>>> DeleteVendor(int vendorId)
	// {
	// 	var response = await _vendorService.RemoveVendorAsync(vendorId);
	// 	if (response.IsSuccess)
	// 	{
	// 		return Ok(response);
	// 	}
	// 	return BadRequest(response);
	// } TODO ??
}
