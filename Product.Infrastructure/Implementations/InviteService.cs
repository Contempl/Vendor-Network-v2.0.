using Microsoft.EntityFrameworkCore;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Implementations;

public class InviteService : IInviteService
{
	private readonly IInviteRepository _inviteRepository;
	private readonly IPasswordHasher _passwordHasher;

	public InviteService(IInviteRepository inviteRepository, IPasswordHasher passwordHasher)
	{
		_inviteRepository = inviteRepository;
		_passwordHasher = passwordHasher;
	}

	public async Task CreateAsync(Invite invite) => await _inviteRepository.CreateAsync(invite);
	public async Task DeleteAsync(Invite invite) => await _inviteRepository.DeleteAsync(invite);
	public async Task<Invite> GetByIdAsync(int inviteId) => await _inviteRepository.GetByIdAsync(inviteId);
	public void ValidateInvite(Invite invite)
	{
		if (invite.ExpiresAt < DateTime.UtcNow)
		{
			throw new Exception($"Invitation expired. Invite id: {invite.Id}");
		}

		if (invite.Status != InvitationStatus.Sent)
		{
			throw new Exception($"Invalid invitation. Invite id: {invite.Id}");
		}
	}
	public async Task<Invite> GetInviteWithUserAsync(int inviteId)
	{
		var invite = await _inviteRepository.GetAll()
			.Where(i => i.Id == inviteId)
			.Include(i => i.User)
			.FirstAsync();

		return invite;
	}
	public async Task UpdateAsync(Invite invite) => await _inviteRepository.UpdateAsync(invite);
	public void UpdateInvitationStatusAsync(User user, Invite invite, UserRegistrationByInviteDto dto)
	{
		invite.Status = InvitationStatus.Accepted;
		
		user.UserName = dto.UserName;
		user.FirstName = dto.FirstName;
		user.LastName = dto.LastName;
		user.PasswordHash = _passwordHasher.HashThePassword(dto.Password);
	}
	public Invite CreateInvite(User user, Administrator admin) => new Invite
	{
		User = user,
		UserId = user.Id,
		Status = InvitationStatus.Sent,
		CreatedAt = DateTime.UtcNow,
		ExpiresAt = DateTime.UtcNow.AddDays(30),
		AdminId = admin.Id,
		Admin = admin
	};
}
