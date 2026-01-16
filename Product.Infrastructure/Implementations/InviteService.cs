using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Infrastructure.Implementations;

public class InviteService : IInviteService
{
	private readonly IInviteRepository _inviteRepository;
	private readonly IUserRepository _userRepository;
	private readonly IOperatorUserRepository _operatorUserRepository;
	private readonly IVendorUserRepository _vendorUserRepository;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ILogger<InviteService> _logger;

	public InviteService(
		IInviteRepository inviteRepository, IUserRepository userRepository, 
		IOperatorUserRepository operatorUserRepository, IVendorUserRepository vendorUserRepository, 
		IPasswordHasher passwordHasher, ILogger<InviteService> logger)
	{
		_inviteRepository = inviteRepository;
		_userRepository = userRepository;
		_operatorUserRepository = operatorUserRepository;
		_vendorUserRepository = vendorUserRepository;
		_passwordHasher = passwordHasher;
		_logger = logger;
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


	public async Task<OneOf<InviteIdToFrontEnd, ValidationError, Error>> RegisterUser(int inviteId, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var invite = await _inviteRepository.GetByIdAsync(inviteId, cancellationToken);
		
			var inviteIsValid = ValidateInvite(invite);

			if (!inviteIsValid)
			{
				_logger.LogWarning("Invalid invitation. Invite id: {invite.Id}.", invite.Id);
				return new ValidationError();
			}
		
			var newUser = new InviteIdToFrontEnd { InviteId = inviteId };
			return newUser;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while registering user.");
			return new Error();
		}
	}
	
	public async Task<OneOf<UserDtoToFrontEnd, ValidationError, NotFoundError, Error>> RegisterByInvite(int inviteId, 
		UserRegistrationByInviteDto registrationData, CancellationToken cancellationToken = default)
	{
		try
		{
			var invite = await _inviteRepository.GetInviteWithUserAsync(inviteId, cancellationToken);
		
			var inviteIsValid = ValidateInvite(invite);

			if (!inviteIsValid)
			{
				_logger.LogWarning("Invalid invitation. Invite id: {invite.Id}.", invite.Id);
				return new ValidationError();
			}
		
			await UpdateInviteAndUser(registrationData, invite, inviteId, cancellationToken);
		
			var updatedUser = await _userRepository.GetByIdOrDefaultAsync(invite.InvitedUserId!.Value);

			if (updatedUser == null)
			{
				_logger.LogWarning("User with id: {inviteId} was not found.", invite.Id);
				return new NotFoundError();
			}

			var result = updatedUser.MapToFrontEndDto();
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while registering user.");
			return new Error();
		}
	}
	
	private async Task UpdateInviteAndUser (UserRegistrationByInviteDto dto, Invite invite, int inviteId, 
		CancellationToken cancellationToken = default)
	{
		var inviteUserId = invite.InvitedUserId!.Value;
		var user = await _userRepository.GetByIdAsync(inviteUserId, cancellationToken);

		user.MapUserToUpdateByInvite(dto);
		user.PasswordHash = _passwordHasher.HashThePassword(dto.Password);

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
	
	private bool ValidateInvite(Invite invite)
	{
		if (invite.ExpiresAt < DateTime.UtcNow || invite.Status != InvitationStatus.Sent)
			return false;
		return true;
	}
}
