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
	
	[AllowAnonymous]
	[HttpPost("Login")]
	public async Task<ActionResult<Response<TokenDto>>> Login (UserLoginDto userData)
	{
		var response = await _adminService.Login(userData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
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
	
	
	[HttpDelete("Vendor/{vendorId}")]
	[EnsureVendorExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<Response<int>>> RemoveVendor(int vendorId)
	{
		var response = await _adminService.RemoveVendorAsync(vendorId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	} 
	
	[HttpDelete("Operator/{operatorId}")]
	[EnsureOperatorExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<Response<int>>> RemoveOperator(int operatorId)
	{
		var response = await _adminService.RemoveOperatorAsync(operatorId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
}
