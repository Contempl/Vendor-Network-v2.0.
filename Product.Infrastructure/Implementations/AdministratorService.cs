using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;

namespace Product.Infrastructure.Implementations;

public class AdministratorService : IAdministratorService
{
    private readonly IAdministratorRepository _adminRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IInviteService _inviteService;
    private readonly IInviteRepository _inviteRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IOperatorRepository _operatorRepository;
    private readonly IUserPrincipalService _userPrincipalService;
    private readonly IVendorUserRepository _vendorUserRepository;
    private readonly IOperatorUserRepository _operatorUserRepository;
    

    public AdministratorService(IAdministratorRepository administratorRepository, IUserRepository userRepository, 
	    IEmailService emailService, IInviteService inviteService, IInviteRepository inviteRepository, IVendorRepository vendorRepository, 
	    IOperatorRepository operatorRepository, IUserPrincipalService userPrincipalService,
	    IVendorUserRepository vendorUserRepository, IOperatorUserRepository operatorUserRepository)
    {
	    _adminRepository = administratorRepository;
	    _userRepository = userRepository;
	    _emailService = emailService;
	    _inviteService = inviteService;
	    _inviteRepository = inviteRepository;
	    _vendorRepository = vendorRepository;
	    _operatorRepository = operatorRepository;
	    _userPrincipalService = userPrincipalService;
	    _vendorUserRepository = vendorUserRepository;
	    _operatorUserRepository = operatorUserRepository;
    }
    

    public async Task<OneOf<UserDtoToFrontEnd, Error>> InviteVendorUser(DataForInviteDto inviteData, 
	    CancellationToken cancellationToken= default)
	{
		try
		{
			var adminId = _userPrincipalService.UserId!.Value;
			var admin = await _adminRepository.GetByIdAsync(adminId, cancellationToken);

			var isValid = ValidateUserInviteData(inviteData);
			if (isValid is false)
				return new Error();
		
			var vendorUserCreationDto = new VendorUserCreationDto
			{
				Email = inviteData.Email, 
				VendorId = inviteData.BusinessId, 
				UserType = UserType.VendorUser,
				CreatedBy = adminId
			};

			var vendorUser = await _vendorUserRepository.CreateAsync(vendorUserCreationDto, cancellationToken);

			var invite = _inviteService.CreateInviteByAdmin(vendorUser, admin);
			await _inviteRepository.CreateAsync(invite, cancellationToken);

			var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
			var emailBody = _emailService.GenerateEmailTemplate(inviteData.Email, vendorUser, inviteUrl); 

			var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

			await _emailService.SendInvitationEmailAsync(mailMessage);

			var responseDto = vendorUser.MapToFrontEndDto();

			return responseDto;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			return new Error();
		}
	}

	public async Task<OneOf<UserDtoToFrontEnd, Error>> InviteOperatorUser(DataForInviteDto inviteData, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var adminId = _userPrincipalService.UserId!.Value;
			var admin = await _adminRepository.GetByIdAsync(adminId, cancellationToken);

			var isValid = ValidateUserInviteData(inviteData);
			if (isValid is false)
				return new Error();

			var operatorUserCreationDto = new OperatorUserCreationDto()
			{
				Email = inviteData.Email, 
				OperatorId = inviteData.BusinessId, 
				UserType = UserType.OperatorUser,
				CreatedBy = adminId
			};

			var operatorUser = await _operatorUserRepository.CreateAsync(operatorUserCreationDto, cancellationToken);

			var invite = _inviteService.CreateInviteByAdmin(operatorUser, admin);
			await _inviteRepository.CreateAsync(invite, cancellationToken);

			var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
			var emailBody = _emailService.GenerateEmailTemplate(inviteData.Email, operatorUser, inviteUrl); 

			var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

			await _emailService.SendInvitationEmailAsync(mailMessage);

			var responseDto = operatorUser.MapToFrontEndDto();

			return responseDto;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			return new Error();
		}
	}

	public async Task<OneOf<UserDtoToFrontEnd, Error>> InviteBusiness(BusinessInvitationData invitationData, 
		CancellationToken cancellationToken = default)
	{
		var adminId = _userPrincipalService.UserId!.Value;
		var admin = await _adminRepository.GetByIdAsync(adminId, cancellationToken);
		
		var isValid = await CreateBusinessWithUserResult(invitationData, cancellationToken);
		if (isValid is false)
			return new Error();

		var existingUser = await _userRepository.GetByEmailAsync(invitationData.UserEmail, cancellationToken);
		var invite = _inviteService.CreateInviteByAdmin(existingUser, admin);
		await _inviteRepository.CreateAsync(invite, cancellationToken);
		var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
		var emailBody = _emailService.GenerateEmailTemplate(invitationData.UserEmail,
			existingUser, inviteUrl); 

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);
		
		var responseDto = existingUser.MapToFrontEndDto();
		return responseDto;
	}
	
	public async Task<OneOf<int, Error>> RemoveOperatorAsync(int operatorId, CancellationToken cancellationToken = default)
	{
		var @operator =  await _operatorRepository.GetByIdAsync(operatorId, cancellationToken);
		await _operatorRepository.DeleteAsync(@operator, cancellationToken);

		return @operatorId;
	}

	public async Task<OneOf<int, Error>> RemoveVendorAsync(int vendorId, CancellationToken cancellationToken = default)
	{
		var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);
		await _vendorRepository.DeleteAsync(vendor, cancellationToken);

		return vendorId;
	}

	private async Task<bool> CreateBusinessWithUserResult (BusinessInvitationData invitationData, 
		CancellationToken cancellationToken = default)
	{
		if (invitationData.BusinessIsVendor)
		{
			var vendor = new Vendor
			{
				BusinessName = invitationData.BusinessName,
				Address = invitationData.BusinessAddress,
				Email = invitationData.BusinessEmail
			};

			await _vendorRepository.CreateAsync(vendor, cancellationToken);

			var vendorUser = new VendorUser
			{
				Email = invitationData.UserEmail,
				FirstName = invitationData.FirstName,
				LastName = invitationData.LastName,
			};

			await _userRepository.CreateAsync(vendorUser, cancellationToken);
		}
		else 
		{
			var @operator = new Operator
			{
				BusinessName = invitationData.BusinessName,
				Address = invitationData.BusinessAddress,
				Email = invitationData.BusinessEmail
			};

			await _operatorRepository.CreateAsync(@operator, cancellationToken);

			var operatorUser = new OperatorUser
			{
				Email = invitationData.UserEmail,
				FirstName = invitationData.FirstName,
				LastName = invitationData.LastName,
			};
			await _userRepository.CreateAsync(operatorUser, cancellationToken);
		}

		return true;
	}

	private bool ValidateUserInviteData(DataForInviteDto userInvitationData) =>
		string.IsNullOrWhiteSpace(userInvitationData.Email) || userInvitationData.BusinessId == 0;
}
