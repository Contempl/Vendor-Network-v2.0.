using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Filters;

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
	private readonly IUserPrincipalService _userPrincipalService;
	private readonly IVendorService _vendorService;
	private readonly IOperatorService _operatorService;


	public AdminController(IInviteService inviteService, IAdministratorService adminService,
		IEmailService emailService, IUserService userService, IUserPrincipalService userPrincipalService, IVendorService vendorService, IOperatorService operatorService)
	{
		_inviteService = inviteService;
		_adminService = adminService;
		_emailService = emailService;
		_userService = userService;
		_userPrincipalService = userPrincipalService;
		_vendorService = vendorService;
		_operatorService = operatorService;
	}

	[HttpPost("inviteBusiness")]
	[EnsureAdministratorExists]
	public async Task<IActionResult> InviteBusiness([FromBody] BusinessInvitationData registrationData)
	{
		var adminId = _userPrincipalService.UserId!.Value;
		var admin = await _adminService.GetByIdAsync(adminId);

		if (registrationData.Business is Vendor vendor)
		{
			vendor.BusinessName = registrationData.BusinessName;
			vendor.Address = registrationData.BusinessAddress;
			vendor.Email = registrationData.BusinessEmail;
			await _vendorService.CreateAsync(vendor);

			var vendorUser = new VendorUser
			{
				Email = registrationData.UserEmail,
				FirstName = registrationData.FirstName,
				LastName = registrationData.LastName,
			};
			await _userService.CreateAsync(vendorUser);
		}
		else if (registrationData.Business is Operator @operator)
		{
			@operator.BusinessName = registrationData.BusinessName;
			@operator.Address = registrationData.BusinessAddress;
			@operator.Email = registrationData.BusinessEmail;
			await _operatorService.CreateAsync(@operator);

			var operatorUser = new OperatorUser
			{
				Email = registrationData.UserEmail,
				FirstName = registrationData.FirstName,
				LastName = registrationData.LastName,
			};
			await _userService.CreateAsync(operatorUser);
		}

		var existingUser = await _userService.GetByEmailAsync(registrationData.UserEmail);
		var invite = _inviteService.CreateInvite(existingUser, admin);
		var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
		var emailBody = _emailService.GenerateEmailTemplate(registrationData.UserEmail,
			existingUser, inviteUrl); 

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);
		return Ok();
	}

	[HttpPost("/inviteVendorUser")]
	[EnsureAdministratorExists]
	public async Task<IActionResult> InviteVendorUser([FromBody] DataForInviteDto inviteData)
	{
		var adminId = _userPrincipalService.UserId;
		var admin = await _adminService.GetByIdAsync(adminId!.Value);
		var vendorUser = new VendorUser { Email = inviteData.Email, VendorId = inviteData.BusinessId };

		await _userService.CreateAsync(vendorUser);

		var existingUser = await _userService.GetByEmailAsync(inviteData.Email);

		var invite = _inviteService.CreateInvite(existingUser, admin);
		await _inviteService.CreateAsync(invite);

		var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
		var emailBody = _emailService.GenerateEmailTemplate(inviteData.Email, existingUser, inviteUrl); 

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);
		return Ok();
	}

	[HttpPost("{adminId}/inviteOperatorUser")]
	[EnsureAdministratorExists]
	public async Task<IActionResult> InviteOperatorUser([FromBody] DataForInviteDto inviteData)
	{
		var adminId = _userPrincipalService.UserId!.Value;
		var admin = await _adminService.GetByIdAsync(adminId);
		var operatorUser = new OperatorUser { Email = inviteData.Email, OperatorId = inviteData.BusinessId };

		await _userService.CreateAsync(operatorUser);

		var existingUser = await _userService.GetByEmailAsync(inviteData.Email);

		var invite = _inviteService.CreateInvite(existingUser, admin);
		var inviteUrl = _emailService.CreateInviteUrl(invite.Id); 
		await _inviteService.CreateAsync(invite);


		var emailBody = _emailService.GenerateEmailTemplate(inviteData.Email, existingUser, inviteUrl);

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);
		return Ok();
	}
}
