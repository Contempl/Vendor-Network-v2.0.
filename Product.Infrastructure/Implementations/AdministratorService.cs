using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;

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
    

    public AdministratorService(IAdministratorRepository administratorRepository, IUserRepository userRepository, IEmailService emailService, IInviteService inviteService, IInviteRepository inviteRepository, IVendorRepository vendorRepository, IOperatorRepository operatorRepository)
    {
	    _adminRepository = administratorRepository;
	    _userRepository = userRepository;
	    _emailService = emailService;
	    _inviteService = inviteService;
	    _inviteRepository = inviteRepository;
	    _vendorRepository = vendorRepository;
	    _operatorRepository = operatorRepository;
    }

	public async Task<Response<UserDtoToFrontEnd>> InviteVendorUser(int adminId, DataForInviteDto inviteData)
	{
		var admin = await _adminRepository.GetByIdAsync(adminId);

		if (admin == null)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = "Administrator not found.",
				ErrorCode = (int)ErrorCodes.UserNotFound,
			};
		}

		if (string.IsNullOrWhiteSpace(inviteData.Email) || inviteData.BusinessId == 0)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = "Invalid user data in invitation",
				ErrorCode = (int)ErrorCodes.InvalidInvitationData
			};
		}
		var vendorUser = new VendorUser { Email = inviteData.Email, VendorId = inviteData.BusinessId };

		await _userRepository.CreateAsync(vendorUser);

		var existingUser = await _userRepository.GetByEmailAsync(inviteData.Email);

		var invite = _inviteService.CreateInvite(existingUser, admin);
		await _inviteRepository.CreateAsync(invite);

		var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
		var emailBody = _emailService.GenerateEmailTemplate(inviteData.Email, existingUser, inviteUrl); 

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);

		var responseDto = existingUser.MapToFrontEndDto();
		
		return new Response<UserDtoToFrontEnd>
		{
			Data = responseDto,
		};
	}

	public async Task<Response<UserDtoToFrontEnd>> InviteOperatorUser(int adminId, DataForInviteDto inviteData)
	{
		var admin = await _adminRepository.GetByIdAsync(adminId);

		if (string.IsNullOrWhiteSpace(inviteData.Email) || inviteData.BusinessId == 0)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = "Invalid user data in invitation",
				ErrorCode = (int)ErrorCodes.InvalidInvitationData
			};
		}
		var vendorUser = new OperatorUser { Email = inviteData.Email, OperatorId = inviteData.BusinessId };

		await _userRepository.CreateAsync(vendorUser);

		var existingUser = await _userRepository.GetByEmailAsync(inviteData.Email);

		var invite = _inviteService.CreateInvite(existingUser, admin);
		await _inviteRepository.CreateAsync(invite);

		var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
		var emailBody = _emailService.GenerateEmailTemplate(inviteData.Email, existingUser, inviteUrl); 

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);

		var responseDto = existingUser.MapToFrontEndDto();
		
		return new Response<UserDtoToFrontEnd>
		{
			Data = responseDto,
		};
	}

	public async Task<Response<UserDtoToFrontEnd>> InviteBusiness(int adminId, BusinessInvitationData invitationData)
	{
		var admin = await _adminRepository.GetByIdAsync(adminId);
		
		if (invitationData.BusinessIsVendor)
		{
			var vendor = new Vendor();
			vendor.BusinessName = invitationData.BusinessName;
			vendor.Address = invitationData.BusinessAddress;
			vendor.Email = invitationData.BusinessEmail;
			
			await _vendorRepository.CreateAsync(vendor);

			var vendorUser = new VendorUser
			{
				Email = invitationData.UserEmail,
				FirstName = invitationData.FirstName,
				LastName = invitationData.LastName,
			};
			await _userRepository.CreateAsync(vendorUser);
		}
		else if (!invitationData.BusinessIsVendor)
		{
			var @operator = new Operator();
			@operator.BusinessName = invitationData.BusinessName;
			@operator.Address = invitationData.BusinessAddress;
			@operator.Email = invitationData.BusinessEmail;
			
			await _operatorRepository.CreateAsync(@operator);

			var operatorUser = new OperatorUser
			{
				Email = invitationData.UserEmail,
				FirstName = invitationData.FirstName,
				LastName = invitationData.LastName,
			};
			await _userRepository.CreateAsync(operatorUser);
		}

		var existingUser = await _userRepository.GetByEmailAsync(invitationData.UserEmail);
		var invite = _inviteService.CreateInvite(existingUser, admin);
		await _inviteRepository.CreateAsync(invite);
		var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
		var emailBody = _emailService.GenerateEmailTemplate(invitationData.UserEmail,
			existingUser, inviteUrl); 

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);
		
		var responseDto = existingUser.MapToFrontEndDto();
		return new Response<UserDtoToFrontEnd>
		{
			Data = responseDto,
		};
	}
}
