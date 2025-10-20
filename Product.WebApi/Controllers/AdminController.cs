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
	private readonly IAuthService _authService;

	public AdminController(IAdministratorService adminService, IUserPrincipalService userPrincipalService, IAuthService authService)
	{
		_adminService = adminService;
		_userPrincipalService = userPrincipalService;
		_authService = authService;
	}
	
	[AllowAnonymous]
	[HttpPost("Login")]
	public async Task<ActionResult<Response<TokenDto>>> Login (UserLoginDto userData, CancellationToken cancellationToken)
	{
		var response = await _authService.LoginAdministrator(userData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("inviteBusiness")]
	public async Task<ActionResult<Response<InviteIdToFrontEnd>>> InviteBusiness([FromBody] BusinessInvitationData invitationData,
		CancellationToken cancellationToken)
	{
		var adminId = _userPrincipalService.UserId!.Value;
		var response = await _adminService.InviteBusiness(adminId, invitationData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("/inviteVendorUser")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> InviteVendorUser([FromBody] DataForInviteDto inviteData,
		CancellationToken cancellationToken)
	{
		var adminId = _userPrincipalService.UserId!.Value;
		var response = await _adminService.InviteVendorUser(adminId, inviteData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("/inviteOperatorUser")]
	public async Task<IActionResult> InviteOperatorUser([FromBody] DataForInviteDto inviteData, CancellationToken cancellationToken)
	{
		var adminId = _userPrincipalService.UserId!.Value;
		var response = await _adminService.InviteOperatorUser(adminId, inviteData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
	
	
	[HttpDelete("Vendor/{vendorId}")]
	[EnsureVendorExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<Response<int>>> RemoveVendor(int vendorId, CancellationToken cancellationToken)
	{
		var response = await _adminService.RemoveVendorAsync(vendorId, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	} 
	
	[HttpDelete("Operator/{operatorId}")]
	[EnsureOperatorExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<Response<int>>> RemoveOperator(int operatorId, CancellationToken cancellationToken)
	{
		var response = await _adminService.RemoveOperatorAsync(operatorId, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
}
