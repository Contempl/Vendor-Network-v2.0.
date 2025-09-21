using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Infrastructure.Implementations;
using Xunit;

namespace Product.Tests;

public class InviteServiceTests
{
	private readonly Mock<IInviteRepository> _inviteRepositoryMock = new();
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly Mock<IOperatorUserRepository> _operatorUserRepositoryMock = new();
	private readonly Mock<IVendorUserRepository> _vendorUserRepositoryMock = new();
	private readonly Mock<IUserService> _userServiceMock = new();
	
	private readonly IInviteService _inviteService;

	public InviteServiceTests()
	{
		_inviteService = new InviteService(
			_inviteRepositoryMock.Object,
			_userRepositoryMock.Object,
			_operatorUserRepositoryMock.Object,
			_vendorUserRepositoryMock.Object,
			_userServiceMock.Object
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
		
		_inviteRepositoryMock.Setup(r => r.GetByIdAsync(invite.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invite);
		
		// Act
		var result = await _inviteService.Register(invite.Id, It.IsAny<CancellationToken>());
		
		// Assert
		Assert.NotNull(result.Data);
		Assert.Equal(invite.Id, result.Data.InviteId);
		_inviteRepositoryMock.Verify(r => r.GetByIdAsync(invite.Id, It.IsAny<CancellationToken>()), Times.Once);	
	}
	
	[Fact]
	public async Task RegisterUser_InvitationExpired_ReturnsErrorResponse()
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
		
		_inviteRepositoryMock.Setup(r => r.GetByIdAsync(invite.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invite);
		
		// Act
		var result = await _inviteService.Register(invite.Id, It.IsAny<CancellationToken>());
		
		// Assert
		Assert.Equal((int)ErrorCodes.InvalidInvitation, result.ErrorCode);
		Assert.Equal($"Invalid invitation. Invite id: {invite.Id}", result.ErrorMessage);
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
		_userRepositoryMock.Setup(r => r.GetByIdAsync(invite.InvitedUserId.Value, It.IsAny<CancellationToken>())).ReturnsAsync(user);
		_inviteRepositoryMock.Setup(r => r.GetInviteWithUserAsync(invite.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(invite);
		_vendorUserRepositoryMock.Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()));
		
		
		// Act
		var result = await _inviteService.RegisterByInvite(invite.Id, registrationData, It.IsAny<CancellationToken>());
		
		// Assert
		Assert.NotNull(result);
		Assert.Equal((int)ErrorCodes.InvalidInvitation, result.ErrorCode);
		Assert.Equal($"Invalid invitation. Invite id: {invite.Id}", result.ErrorMessage);
	}
}