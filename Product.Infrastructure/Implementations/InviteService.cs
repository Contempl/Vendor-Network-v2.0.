using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;

namespace Product.Infrastructure.Implementations;

public class InviteService : IInviteService
{
	private readonly IInviteRepository _inviteRepository;
	private readonly IUserRepository _userRepository;
	private readonly IOperatorUserRepository _operatorUserRepository;
	private readonly IVendorUserRepository _vendorUserRepository;
	private readonly IUserService _userService;

	public InviteService(IInviteRepository inviteRepository, IUserRepository userRepository, IOperatorUserRepository operatorUserRepository, IVendorUserRepository vendorUserRepository, IUserService userService)
	{
		_inviteRepository = inviteRepository;
		_userRepository = userRepository;
		_operatorUserRepository = operatorUserRepository;
		_vendorUserRepository = vendorUserRepository;
		_userService = userService;
	}
	
	private bool ValidateInvite(Invite invite)
	{
		if (invite.ExpiresAt < DateTime.UtcNow || invite.Status != InvitationStatus.Sent)
			return false;
		return true;
	}
	public Invite CreateInvite(User user, User sender) => new Invite
	{
		InvitedUser = user,
		Status = InvitationStatus.Sent,
		CreatedAt = DateTime.UtcNow,
		ExpiresAt = DateTime.UtcNow.AddDays(30),
		SenderId = sender.Id
	};

	public Invite CreateInviteByAdmin(User user, Administrator sender) => new Invite
	{
		InvitedUser = user,
		Status = InvitationStatus.Sent,
		CreatedAt = DateTime.UtcNow,
		ExpiresAt = DateTime.UtcNow.AddDays(30),
		SenderId = sender.Id
	};


	public async Task<Response<InviteIdToFrontEnd>> RegisterUser(int inviteId, 
		CancellationToken cancellationToken = default)
	{
		var invite = await _inviteRepository.GetByIdAsync(inviteId, cancellationToken);
		
		var inviteIsValid = ValidateInvite(invite);

		if (!inviteIsValid)
		{
			return new Response<InviteIdToFrontEnd>
			{
				ErrorMessage = $"Invalid invitation. Invite id: {invite.Id}",
				ErrorCode = (int)ErrorCodes.InvalidInvitation
			};
		}
		
		var newUser = new InviteIdToFrontEnd { InviteId = inviteId };
		return new Response<InviteIdToFrontEnd>
		{
			Data = newUser
		};
	}
	
	public async Task<Response<UserDtoToFrontEnd>> RegisterByInvite(int inviteId, 
		UserRegistrationByInviteDto registrationData, CancellationToken cancellationToken = default)
	{
		var invite = await _inviteRepository.GetInviteWithUserAsync(inviteId, cancellationToken);
		
		var inviteIsValid = ValidateInvite(invite);

		if (!inviteIsValid)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = $"Invalid invitation. Invite id: {invite.Id}",
				ErrorCode = (int)ErrorCodes.InvalidInvitation
			};
		}
		
		await UpdateInviteAndUser(registrationData, invite, inviteId, cancellationToken);
		
		var updatedUser = await _userRepository.GetByIdOrDefaultAsync(invite.InvitedUserId!.Value);

		return new Response<UserDtoToFrontEnd>
		{
			Data = updatedUser.MapToFrontEndDto()
		};
	}
	
	private async Task UpdateInviteAndUser (UserRegistrationByInviteDto dto, Invite invite, int inviteId, 
		CancellationToken cancellationToken = default)
	{
		var inviteUserId = invite.InvitedUserId!.Value;
		var user = await _userRepository.GetByIdAsync(inviteUserId, cancellationToken);

		_userService.MapUserToUpdateByInvite(dto, user);

		if (user is VendorUser vendorUser)
		{
			await _vendorUserRepository.UpdateAsync(vendorUser, cancellationToken);
		}
		else if (user is OperatorUser operatorUser)
		{
			await _operatorUserRepository.UpdateAsync(operatorUser, cancellationToken);
		}
		else
		{
			throw new InvalidOperationException("Unknown user type");
		}
		
		invite.Id = inviteId;
		invite.Status = InvitationStatus.Accepted;
		await _inviteRepository.UpdateAsync(invite, cancellationToken);
	}
}
