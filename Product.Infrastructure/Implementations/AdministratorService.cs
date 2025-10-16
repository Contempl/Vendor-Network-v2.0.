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
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    

    public AdministratorService(IAdministratorRepository administratorRepository, IUserRepository userRepository, 
	    IEmailService emailService, IInviteService inviteService, IInviteRepository inviteRepository, IVendorRepository vendorRepository, 
	    IOperatorRepository operatorRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
	    _adminRepository = administratorRepository;
	    _userRepository = userRepository;
	    _emailService = emailService;
	    _inviteService = inviteService;
	    _inviteRepository = inviteRepository;
	    _vendorRepository = vendorRepository;
	    _operatorRepository = operatorRepository;
	    _passwordHasher = passwordHasher;
	    _jwtTokenService = jwtTokenService;
    }


    public async Task<Response<TokenDto>> Login(UserLoginDto userData, CancellationToken cancellationToken = default)
    {
	    var admin = await _adminRepository.GetByEmailAsync(userData.Email, cancellationToken);

	    if (admin == null)
	    {
		    return new Response<TokenDto>
		    {
			    ErrorMessage = "Admin not found",
			    ErrorCode = (int)ErrorCodes.UserNotFound
		    };
	    }

	    var passwordsAreEqual = _passwordHasher.ValidatePassword(userData.Password, admin.PasswordHash!);

	    if (!passwordsAreEqual)
	    {
		    return new Response<TokenDto>
		    {
			    ErrorMessage = "Invalid password",
			    ErrorCode = (int)ErrorCodes.InvalidPassword
		    };
	    }

	    var userClaims = admin.MapAdminToClaimDto();
	    var token = _jwtTokenService.GenerateToken(userClaims);

	    return new Response<TokenDto>
	    {
		    Data = token,
	    };
    }

    public async Task<Response<UserDtoToFrontEnd>> InviteVendorUser(int adminId, DataForInviteDto inviteData, 
	    CancellationToken cancellationToken = default)
	{
		var admin = await _adminRepository.GetByIdOrDefaultAsync(adminId);

		if (admin == null)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = "Admin not found.",
				ErrorCode = (int)ErrorCodes.UserNotFound
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
		var vendorUser = new VendorUser { Email = inviteData.Email, VendorId = inviteData.BusinessId, UserType = UserType.VendorUser };

		await _userRepository.CreateAsync(vendorUser, cancellationToken);

		var invite = _inviteService.CreateInviteByAdmin(vendorUser, admin);
		await _inviteRepository.CreateAsync(invite, cancellationToken);

		var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
		var emailBody = _emailService.GenerateEmailTemplate(inviteData.Email, vendorUser, inviteUrl); 

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);

		var responseDto = vendorUser.MapToFrontEndDto();
		
		return new Response<UserDtoToFrontEnd>
		{
			Data = responseDto,
		};
	}

	public async Task<Response<UserDtoToFrontEnd>> InviteOperatorUser(int adminId, DataForInviteDto inviteData, 
		CancellationToken cancellationToken = default)
	{
		var admin = await _adminRepository.GetByIdOrDefaultAsync(adminId);

		if (admin == null)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = "Admin not found.",
				ErrorCode = (int)ErrorCodes.UserNotFound
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
		
		var operatorUser = new OperatorUser { Email = inviteData.Email, OperatorId = inviteData.BusinessId };

		await _userRepository.CreateAsync(operatorUser, cancellationToken);

		var invite = _inviteService.CreateInviteByAdmin(operatorUser, admin);
		await _inviteRepository.CreateAsync(invite, cancellationToken);

		var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
			
		var emailBody = _emailService.GenerateEmailTemplate(inviteData.Email, operatorUser, inviteUrl); 

		var mailMessage = _emailService.CreateMessage(emailBody, admin.Email);

		await _emailService.SendInvitationEmailAsync(mailMessage);

		var responseDto = operatorUser.MapToFrontEndDto();

		return new Response<UserDtoToFrontEnd>
		{
			Data = responseDto,
		};
	}

	public async Task<Response<UserDtoToFrontEnd>> InviteBusiness(int adminId, BusinessInvitationData invitationData, 
		CancellationToken cancellationToken = default)
	{
		var admin = await _adminRepository.GetByIdOrDefaultAsync(adminId);

		if (admin == null)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = "Admin not found.",
				ErrorCode = (int)ErrorCodes.UserNotFound
			};
		};
		
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
		else if (!invitationData.BusinessIsVendor)
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

		var existingUser = await _userRepository.GetByEmailAsync(invitationData.UserEmail, cancellationToken);
		var invite = _inviteService.CreateInviteByAdmin(existingUser, admin);
		await _inviteRepository.CreateAsync(invite, cancellationToken);
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
	
	public async Task<Response<int>> RemoveOperatorAsync(int operatorId, CancellationToken cancellationToken = default)
	{
		var @operator =  await _operatorRepository.GetByIdAsync(operatorId, cancellationToken);
		await _operatorRepository.DeleteAsync(@operator, cancellationToken);
		 
		return new Response<int>
		{
			Data = @operator.Id
		};
	}

	public async Task<Response<int>> RemoveVendorAsync(int vendorId, CancellationToken cancellationToken = default)
	{
		var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);
		await _vendorRepository.DeleteAsync(vendor, cancellationToken);
		 
		return new Response<int>
		{
			Data = vendor.Id
		};
	}
}
