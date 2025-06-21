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
		InvitedUserId = user.Id,
		Status = InvitationStatus.Sent,
		CreatedAt = DateTime.UtcNow,
		ExpiresAt = DateTime.UtcNow.AddDays(30),
		SenderId = sender.Id,
		Sender = sender
	};

	public async Task<Response<InviteIdToFrontEnd>> Register(int inviteId)
	{
		var invite = await _inviteRepository.GetByIdAsync(inviteId);
		
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
	
	public async Task<Response<UserDtoToFrontEnd>> RegisterByInvite(int inviteId, UserRegistrationByInviteDto registrationData)
	{
		var invite = await _inviteRepository.GetInviteWithUserAsync(inviteId);
		
		var inviteIsValid = ValidateInvite(invite);

		if (!inviteIsValid)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = $"Invalid invitation. Invite id: {invite.Id}",
				ErrorCode = (int)ErrorCodes.InvalidInvitation
			};
		}
		
		await UpdateInviteAndUser(registrationData, invite, inviteId);
		
		var updatedUser = await _userRepository.GetByIdOrDefaultAsync(invite.InvitedUserId.Value);

		return new Response<UserDtoToFrontEnd>
		{
			Data = updatedUser.MapToFrontEndDto()
		};
	}
	
	private async Task UpdateInviteAndUser (UserRegistrationByInviteDto dto, Invite invite, int inviteId)
	{
		var user = await _userRepository.GetByIdAsync(invite.InvitedUserId.Value);

		_userService.MapUserToUpdateByInvite(dto, user);

		if (user is VendorUser vendorUser)
		{
			await _vendorUserRepository.UpdateAsync(vendorUser);
		}
		else if (user is OperatorUser operatorUser)
		{
			await _operatorUserRepository.UpdateAsync(operatorUser);
		}
		else
		{
			throw new InvalidOperationException("Unknown user type");
		}
		invite.Id = inviteId;
		invite.Status = InvitationStatus.Accepted;
		 await _inviteRepository.UpdateAsync(invite);
	}
}
