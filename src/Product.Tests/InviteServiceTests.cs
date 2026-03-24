using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;
using Product.Infrastructure.Implementations;
using Xunit;

namespace Product.Tests;

public class InviteServiceTests
{
	private readonly Mock<IInviteRepository> _inviteRepositoryMock = new();
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly Mock<IOperatorUserRepository> _operatorUserRepositoryMock = new();
	private readonly Mock<IVendorUserRepository> _vendorUserRepositoryMock = new();
	private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
	private readonly Mock<ILogger<InviteService>> _loggerMock = new();
	
	private readonly IInviteService _inviteService;

	public InviteServiceTests()
	{
		_inviteService = new InviteService(
			_inviteRepositoryMock.Object,
			_userRepositoryMock.Object,
			_operatorUserRepositoryMock.Object,
			_vendorUserRepositoryMock.Object,
			_passwordHasherMock.Object,
			_loggerMock.Object
		);
	}

	[Fact]
	public void CreateInvite_WithValidUserAndSender_ReturnsValidInvite()
	{
		// Arrange
		var user = new VendorUser { Id = 10, Email = "user@test.com" };
		var sender = new Administrator { Id = 1, Email = "admin@test.com" };

		// Act
		var invite = _inviteService.CreateInviteByAdmin(user, sender);

		// Assert
		Assert.NotNull(invite);
		Assert.Equal(user, invite.InvitedUser);
		Assert.Equal(sender.Id, invite.SenderId);
		Assert.Equal(InvitationStatus.Sent, invite.Status);
		Assert.True(invite.CreatedAt <= DateTime.UtcNow);
	}

	[Fact]
	public async Task RegisterInvite_WithValidInviteId_ReturnsDataSuccessfully()
	{
		// Arrange
		var user = new VendorUser { Id = 10, Email = "user@test.com" };
		var sender = new Administrator { Id = 1, Email = "admin@test.com" };
		var invite = new Invite
		{
			Id = 1,
			CreatedAt = DateTime.UtcNow,
			Status = InvitationStatus.Sent,
			SenderId = sender.Id,
			Sender = sender,
			InvitedUser = user,
			InvitedUserId = user.Id
		};
		
		_inviteRepositoryMock.Setup(r => r.GetByIdAsync(invite.Id, CancellationToken.None)).ReturnsAsync(invite);
		
		// Act
		var result = await _inviteService.GetInviteById(invite.Id, CancellationToken.None);
		
		// Assert
		Assert.True(result.Value is InviteDtoWithStatus);
		var resultDto = result.Value as InviteDtoWithStatus;
		Assert.Equal(invite.Id, resultDto!.InviteId);
		Assert.Equal(invite.Status.ToString(), resultDto!.Status);
		_inviteRepositoryMock.Verify(r => r.GetByIdAsync(invite.Id, CancellationToken.None), Times.Once);	
	}
	
	[Fact]
	public async Task RegisterUser_InvitationExpired_ReturnsExpiredInvite()
	{
		// Arrange
		var user = new VendorUser { Id = 10, Email = "user@test.com" };
		var sender = new Administrator { Id = 1, Email = "admin@test.com" };
		var invite = new Invite
		{
			Id = 1,
			CreatedAt = DateTime.UtcNow,
			Status = InvitationStatus.Expired,
			SenderId = sender.Id,
			Sender = sender,
			InvitedUser = user,
			InvitedUserId = user.Id
		};
		
		_inviteRepositoryMock.Setup(r => r.GetByIdAsync(invite.Id, CancellationToken.None)).ReturnsAsync(invite);
		
		// Act
		var result = await _inviteService.GetInviteById(invite.Id, CancellationToken.None);
		
		// Assert
		Assert.True(result.Value is InviteDtoWithStatus);
		var resultDto = result.Value as InviteDtoWithStatus;
		Assert.Equal(nameof(InvitationStatus.Expired), resultDto!.Status);
	}

	[Fact]
	public async Task RegisterUserByInvite_InvitationExpired_ReturnsErrorResponse()
	{
		// Arrange
		var user = new VendorUser { Id = 10, Email = "user@test.com" };
		var sender = new Administrator { Id = 1, Email = "admin@test.com" };
		var invite = new Invite
		{
			Id = 1,
			CreatedAt = DateTime.UtcNow,
			Status = InvitationStatus.Expired,
			SenderId = sender.Id,
			Sender = sender,
			InvitedUser = user,
			InvitedUserId = user.Id
		};
	
		var registrationData = new UserRegistrationByInviteDto
		{
			UserName = "Test",
			FirstName = "Test",
			LastName = "Test",
			Password = "password",
		};
		_userRepositoryMock.Setup(r => r.GetByIdAsync(invite.InvitedUserId.Value, CancellationToken.None)).ReturnsAsync(user);
		_inviteRepositoryMock.Setup(r => r.GetInviteWithUserAsync(invite.Id, CancellationToken.None))
			.ReturnsAsync(invite);
		_vendorUserRepositoryMock.Setup(r => r.UpdateAsync(user, CancellationToken.None));
		
		
		// Act
		var result = await _inviteService.RegisterByInvite(invite.Id, registrationData, CancellationToken.None);
		
		// Assert
		Assert.True(result.Value is ValidationError);
	}
}