using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly IInviteService _inviteService;
	private readonly IOperatorUserService _operatorUserService;
	private readonly IVendorUserService _vendorUserService;
	private readonly IAdministratorService _adminService;

	public AccountController(IInviteService inviteService, IVendorUserService vendorUserService,
		IOperatorUserService operatorUserService, IUserService userService, IAdministratorService adminService)
	{
		_inviteService = inviteService;
		_vendorUserService = vendorUserService;
		_operatorUserService = operatorUserService;
		_userService = userService;
		_adminService = adminService;
	}

	[HttpGet("Register/User/{inviteId}")]
	[EnsureInviteExists]
	public async Task<IActionResult> RegisterUser (int inviteId)
	{
		var invite = await _inviteService.GetByIdAsync(inviteId);

		_inviteService.ValidateInvite(invite);

		var newUser = new UserRegistrationDto { InviteId = inviteId };
		return Ok(newUser);
	}


	[HttpPost("Register/User/{inviteId}")]
	[EnsureInviteExists]
	public async Task<IActionResult> RegisterUserByInvite (int inviteId, 
		[FromBody] UserRegistrationByInviteDto registrationData)
	{
		var invite = await _inviteService.GetInviteWithUserAsync(inviteId);

		_inviteService.ValidateInvite(invite);
		
		await UpdateInviteAndUser(registrationData, invite, inviteId);

		return Ok();
	}

	[HttpPost("Register/User")]
	public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto registrationData)
	{
		if (!registrationData.IsOperator)
		{
			var newVendor = _vendorUserService.MapVendorUserFromDto(registrationData);

			await _vendorUserService.CreateAsync(newVendor);
			return Ok();
		}
		
		var newOperator = _operatorUserService.MapOperatorUserFromDto(registrationData);

		await _operatorUserService.CreateAsync(newOperator);
		return Ok();
	}


	[HttpGet("User/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "All")]
	public async Task<ActionResult> GetUser(int userId)
	{
		var existingUser = await _userService.GetByIdAsync(userId);
		var userDto = _userService.MapUserToDto(existingUser);
		return Ok(userDto);
	}

	[HttpPost("Register/User/Admin")]
	public async Task<ActionResult> RegisterAdmin(AdminRegistrationDto registrationData)
	{
		var admin = _adminService.MapAdminFromDto(registrationData);

		await _adminService.CreateAsync(admin);

		return Ok(admin);
	}

	private async Task UpdateInviteAndUser (UserRegistrationByInviteDto dto, Invite invite, int inviteId)
	{
		var user = await _userService.GetByIdAsync((int)invite.UserId!);

		_userService.MapUserToUpdate(dto, user);

		if (user is VendorUser vendorUser)
		{
			await _vendorUserService.UpdateAsync(vendorUser);
		}
		else if (user is OperatorUser operatorUser)
		{
			await _operatorUserService.UpdateAsync(operatorUser);
		}
		else
		{
			throw new InvalidOperationException("Unknown user type");
		}
		invite.Id = inviteId;
		await _inviteService.UpdateAsync(invite);
	}

	[HttpPost("Login")]
	public async Task<ActionResult> Login (UserLoginDto userData)
	{
		var token = await _userService.Login(userData);

		if (string.IsNullOrEmpty(token))
		{
			return NotFound(new { error = "User not found or password invalid" });
		}
		
		return Ok(token);
	}
}
