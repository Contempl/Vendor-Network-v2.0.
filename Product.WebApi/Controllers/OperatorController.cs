using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Filters;
using Product.WebApi.Pagination;

namespace Product.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class OperatorController : Controller
{
	private readonly IOperatorService _operatorService;
	private readonly IOperatorUserService _operatorUserService;
	private readonly IUserPrincipalService _userPrincipalService;
	private readonly IUserService _userService;
	private readonly IInviteService _inviteService;
	private readonly IEmailService _emailService;

	public OperatorController(IOperatorService operatorService, IOperatorUserService operatorUserService, IUserPrincipalService userPrincipalService, IUserService userService, IInviteService inviteService, IEmailService emailService)
	{
		_operatorService = operatorService;
		_operatorUserService = operatorUserService;
		_userPrincipalService = userPrincipalService;
		_userService = userService;
		_inviteService = inviteService;
		_emailService = emailService;
	}

	[HttpPost("search")]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<IActionResult> GetVendorsToServeFacilities(string serviceType, // can't have 2 [FromBody]
	   [FromBody] List<int> operatorLocationsId)
	{
		_operatorService.ValidateStringInput(serviceType);

		var operatorFacilities = _operatorService.GetAllOperatorIndustries(operatorLocationsId);

		var vendors = await _operatorService.SearchVendorsAsync(serviceType, operatorFacilities);

		return Ok(vendors);
	}

	[HttpGet("search/{vendorName}/{pageSize}/{pageNumber}")]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<PagedList<Vendor>>> GetVendors([FromBody]string vendorName,
		SortOrder sortOrder = SortOrder.Ascending, int pageSize = 10, int pageNumber = 1)
	{
		_operatorService.ValidateStringInput(vendorName);

		var pagedVendors = await _operatorService.GetVendorsQuery(vendorName, sortOrder,
			pageSize, pageNumber);

		return Ok(new PagedList<Vendor>(pagedVendors.Items, pageNumber, pageSize, pagedVendors.TotalCount));
	}


	[HttpGet]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Operator>> GetOperator()
	{
		var operatorId = _userPrincipalService.BusinessId;
		var @operator = await _operatorService.GetByIdAsync(operatorId!.Value);

		return Ok(@operator);
	}

	[HttpPost("register/{operatorUserId}")]
	[EnsureOperatorUserExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Operator>> RegisterOperator(int operatorUserId, [FromBody] OperatorRegistrationDto operatorRegistrationData)
	{
		var user = await _operatorUserService.GetByIdAsync(operatorUserId);

		var newOperator = _operatorService.MapOperatorFromDto(operatorRegistrationData, user);

		await _operatorService.CreateAsync(newOperator);
		return CreatedAtAction(nameof(GetOperator), new { operatorId = newOperator.Id }, newOperator);
	}

	[HttpPut]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<IActionResult> UpdateOperator([FromBody] UpdateOperatorDto operatorData)
	{
		var operatorId = _userPrincipalService.BusinessId;
		var @operator = await _operatorService.GetByIdAsync(operatorId!.Value);

		_operatorService.MapOperatorFromDto(@operator, operatorData);

		await _operatorService.UpdateAsync(@operator);

		return Ok(@operator);
	}

	[HttpDelete("{operatorId}")]
	[EnsureOperatorExists]
	[Authorize(policy: "AdminOnly")]
	public async Task<IActionResult> DeleteOperator(int operatorId)
	{
		var @operator = await _operatorService.GetByIdAsync(operatorId);

		await _operatorService.DeleteAsync(@operator);

		return NoContent();
	}

	[HttpPost("invite")]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<IActionResult> InviteOperatorUser([FromBody] string email)
	{
		var operatorUserId = _userPrincipalService.UserId!.Value;
		var operatorUser = await _userService.GetByIdAsync(operatorUserId);
		
		var operatorId = _userPrincipalService.BusinessId;
		
		var newOperatorUser = new OperatorUser { Email = email, OperatorId = operatorId };
		
		await _operatorUserService.CreateAsync(newOperatorUser);
		
		var existingUser = await _userService.GetByEmailAsync(email);
		
		var invite =  _inviteService.CreateInvite(existingUser, operatorUser);
		var inviteUrl = _emailService.CreateInviteUrl(invite.Id); 
		await _inviteService.CreateAsync(invite);
		
		var emailBody = _emailService.GenerateEmailTemplate(email, existingUser, inviteUrl);

		var mailMessage = _emailService.CreateMessage(emailBody, operatorUser.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);
		return Ok();
	}
}
