using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.FIlters;

namespace Product.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(policy: "AdminOnly")]
public class AdminController : ControllerBase
{
	private readonly IInviteService _inviteService;
	private readonly IAdministratorService _adminService;
	private readonly IEmailService _emailService;
	private readonly IUserService _userService;
	private readonly IConfiguration _configuration;


	public AdminController(IInviteService inviteService, IAdministratorService adminService,
		IEmailService emailService, IUserService userService, IConfiguration configuration)
	{
		_inviteService = inviteService;
		_adminService = adminService;
		_emailService = emailService;
		_userService = userService;
		_configuration = configuration;
	}

	[HttpPost("{adminId}/inviteVendor")]
	[EnsureAdministratorExists]
	public async Task<IActionResult> InviteVendorUser(int adminId, [FromBody] string email)
	{
		var admin = await _adminService.GetByIdAsync(adminId);
		var vendorUser = new VendorUser { Email = email };

		await _userService.CreateAsync(vendorUser);

		var existingUser = await _userService.GetByEmailAsync(email);

		var invite = _inviteService.CreateInvite(existingUser, admin);
		await _inviteService.CreateAsync(invite);

		var inviteUrl = CreateInviteUrl(invite.Id);
			
		var emailBody = _adminService.GenerateEmailTemplate(email, existingUser, inviteUrl); 

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);
		return Ok();
	}

	[HttpPost("{adminId}/inviteOperator")]
	[EnsureAdministratorExists]
	public async Task<IActionResult> InviteOperatorUser(int adminId, [FromBody] string email)
	{
		var admin = await _adminService.GetByIdAsync(adminId);
		var operatorUser = new OperatorUser { Email = email };

		await _userService.CreateAsync(operatorUser);

		var existingUser = await _userService.GetByEmailAsync(email);

		var invite = _inviteService.CreateInvite(existingUser, admin);
		var inviteUrl = CreateInviteUrl(invite.Id); 
		await _inviteService.CreateAsync(invite);


		var emailBody = _adminService.GenerateEmailTemplate(email, existingUser, inviteUrl);

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);
		return Ok();
	}
	
	private string CreateInviteUrl(int inviteId)
	{
		var host = _configuration["Host"];

		return string.Concat(host, Url.Action("RegisterUser", "Account", new { inviteId }));
	}
}
