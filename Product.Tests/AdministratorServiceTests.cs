using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Types;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;
using Product.Infrastructure.Implementations;
using Xunit;
using static Product.Domain.Entity.InvitationStatus;

namespace Product.Tests;

public class AdministratorServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IInviteRepository> _inviteRepositoryMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IAdministratorRepository> _adminRepositoryMock = new();
    private readonly Mock<IInviteService> _inviteServiceMock = new();
    private readonly Mock<IVendorRepository> _vendorRepositoryMock = new();
    private readonly Mock<IOperatorRepository> _operatorRepositoryMock = new();
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new();
    private readonly Mock<IVendorUserRepository> _vendorUserRepositoryMock = new();
    private readonly Mock<IOperatorUserRepository> _operatorUserRepositoryMock = new();
    private readonly Mock<ILogger<AdministratorService>> _loggerMock = new();
    

    private readonly AdministratorService _adminService;

    public AdministratorServiceTests()
    {
        _adminService = new AdministratorService(
            _adminRepositoryMock.Object,
            _userRepositoryMock.Object,
            _emailServiceMock.Object,
            _inviteServiceMock.Object,
            _inviteRepositoryMock.Object,
            _vendorRepositoryMock.Object,
            _operatorRepositoryMock.Object,
            _userPrincipalServiceMock.Object,
            _vendorUserRepositoryMock.Object,
            _operatorUserRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task InviteVendorUser_ValidData_ReturnsSuccess()
    {
        // Arrange
        var inviteDto = new DataForInviteDto { Email = "test@example.com", BusinessId = 1 };
        var admin = new Administrator
            { Id =  _testAdmin.Id, FirstName = "Test", LastName = "Test", Email = "test@example.com" };

        var createdUser = new VendorUser
        {
            Id = 123,
            Email = inviteDto.Email,
            VendorId = inviteDto.BusinessId,
            FirstName = "Invited",
            LastName = "User"
        };

        var invite = new Invite
        {
            Id = 456,
            InvitedUserId = createdUser.Id,
            InvitedUser = createdUser,
            SenderId = admin.Id,
            Sender = admin,
            CreatedAt = DateTime.UtcNow,
            Status = Sent,
        };

        _adminRepositoryMock
            .Setup(r => r.GetByIdAsync(_testAdmin.Id, CancellationToken.None))
            .ReturnsAsync(admin);

        _inviteServiceMock
            .Setup(s => s.CreateInviteByAdmin(It.IsAny<User>(), It.IsAny<Administrator>()))
            .Returns(invite);

        _vendorUserRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<VendorUserCreationDto>(), CancellationToken.None))
            .ReturnsAsync(createdUser);
        
        _userPrincipalServiceMock
            .SetupProperty(p => p.UserId, _testAdmin.Id);
        
        _emailServiceMock
            .Setup(e => e.CreateInviteUrl(It.IsAny<int>()))
            .Returns("https://invite.url/token");;

        // Act
        var result = await _adminService.InviteVendorUser(inviteDto, CancellationToken.None);

        // Assert
        Assert.True(result.Value is UserDtoToFrontEnd);
        _inviteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invite>(), CancellationToken.None), Times.Once);
        _emailServiceMock.Verify(e => e.SendInvitationEmailAsync(It.IsAny<MailMsg>()), Times.Once);
    }


    [Fact]
    public async Task InviteVendorUser_ValidData_ReturnsUserDto()
    {
        // Arrange
        var inviteDto = new DataForInviteDto { Email = "vendor@example.com", BusinessId = 10 };

        var admin = new Administrator
        {
            Id =  _testAdmin.Id,
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var createdUser = new VendorUser
        {
            Id = 100,
            Email = inviteDto.Email,
            VendorId = inviteDto.BusinessId
        };

        var invite = new Invite
        {
            Id = 456,
            InvitedUserId = createdUser.Id,
            InvitedUser = createdUser,
            SenderId = admin.Id,
            Sender = admin,
            CreatedAt = DateTime.UtcNow,
            Status = Sent,
        };

        var ct = CancellationToken.None;

        _adminRepositoryMock.Setup(r => r.GetByIdAsync(_testAdmin.Id, CancellationToken.None))
            .ReturnsAsync(admin);

        _vendorUserRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<VendorUserCreationDto>(), CancellationToken.None))
            .ReturnsAsync(createdUser);
        
        _userPrincipalServiceMock
            .SetupProperty(p => p.UserId, _testAdmin.Id);

        _inviteServiceMock
            .Setup(s => s.CreateInviteByAdmin(It.IsAny<User>(), It.IsAny<Administrator>()))
            .Returns(invite);

        _emailServiceMock.Setup(e => e.CreateInviteUrl(invite.Id))
            .Returns("https://invite.url/token");

        _emailServiceMock.Setup(e => e.GenerateEmailTemplate(inviteDto.Email, createdUser, "https://invite.url/token"))
            .Returns("Email Body");

        _emailServiceMock.Setup(e => e.CreateMessage("Email Body", admin.Email))
            .Returns(new MailMsg("Email Body", admin.Email));

        // Act
        var result = await _adminService.InviteVendorUser(inviteDto, ct);

        // Assert
        Assert.True(result.Value is UserDtoToFrontEnd);
        Assert.Equal(inviteDto.Email, result.AsT0.Email);
        _vendorUserRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<VendorUserCreationDto>(), CancellationToken.None), Times.Once);
        _inviteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invite>(), CancellationToken.None), Times.Once);
        _emailServiceMock.Verify(e => e.SendInvitationEmailAsync(It.IsAny<MailMsg>()), Times.Once);
    }

    [Fact]
    public async Task InviteOperatorUser_ValidData_ReturnsUserDto()
    {
        // Arrange
        var inviteDto = new DataForInviteDto { Email = "operator@example.com", BusinessId = 20 };


        _adminRepositoryMock.Setup(r => r.GetByIdAsync(_testAdmin.Id, CancellationToken.None))
            .ReturnsAsync(_testAdmin);

        _operatorUserRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<OperatorUserCreationDto>(), CancellationToken.None))
            .ReturnsAsync(_testOperatorUser);

        _inviteServiceMock
            .Setup(s => s.CreateInviteByAdmin(It.IsAny<User>(), It.IsAny<Administrator>()))
            .Returns(_testInvite);
        
        _userPrincipalServiceMock
            .SetupProperty(p => p.UserId, _testAdmin.Id);

        _emailServiceMock.Setup(e => e.CreateInviteUrl(_testInvite.Id))
            .Returns("https://invite.url/operator-token");

        _emailServiceMock.Setup(e =>
                e.GenerateEmailTemplate(inviteDto.Email, _testOperatorUser, "https://invite.url/operator-token"))
            .Returns("Operator Email Body");

        _emailServiceMock.Setup(e => e.CreateMessage("Operator Email Body", _testAdmin.Email))
            .Returns(new MailMsg("Operator Email Body", _testAdmin.Email));

        // Act
        var result = await _adminService.InviteOperatorUser(inviteDto, CancellationToken.None);

        // Assert
        Assert.True(result.Value is UserDtoToFrontEnd);
        Assert.Equal(inviteDto.Email, result.AsT0.Email);
        _operatorUserRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<OperatorUserCreationDto>(), CancellationToken.None), Times.Once);
        _inviteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invite>(), CancellationToken.None), Times.Once);
        _emailServiceMock.Verify(e => e.SendInvitationEmailAsync(It.IsAny<MailMsg>()), Times.Once);
    }

    [Fact]
    public async Task InviteBusiness_ValidData_CreatesInviteAndSendsEmail()
    {
        // Arrange
        var invitationData = new BusinessInvitationData
        {
            BusinessEmail = "business@example.com",
            BusinessName = "Test Business",
            UserEmail = "Test@example.com",
            BusinessIsVendor = true
        };

        var invite = new Invite
        {
            Id = 789,
            InvitedUserId = _testVendorUser.Id,
            SenderId = _testAdmin.Id,
            Sender = _testAdmin,
            CreatedAt = DateTime.UtcNow,
            Status = Sent
        };

        _adminRepositoryMock.Setup(r => r.GetByIdAsync( _testAdmin.Id, CancellationToken.None))
            .ReturnsAsync(_testAdmin);

         _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<VendorUser>(), CancellationToken.None));

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(invitationData.UserEmail, CancellationToken.None))
            .ReturnsAsync(_testVendorUser);
        
        _userPrincipalServiceMock
            .SetupProperty(p => p.UserId, _testAdmin.Id);

        _inviteServiceMock.Setup(s => s.CreateInviteByAdmin(_testVendorUser, _testAdmin))
            .Returns(invite);

        _inviteRepositoryMock.Setup(r => r.CreateAsync(invite, CancellationToken.None));

        _emailServiceMock.Setup(e => e.CreateInviteUrl(invite.Id))
            .Returns("https://invite-link.com");
        
        _emailServiceMock
            .Setup(e => e.GenerateEmailTemplate(invitationData.UserEmail, _testVendorUser, "https://invite-link.com"))
            .Returns("Email body");

        _emailServiceMock.Setup(e =>
                e.GenerateEmailTemplate(invitationData.BusinessEmail, _testVendorUser, "https://invite-link.com"))
            .Returns("Email body");

        _emailServiceMock.Setup(e => e.CreateMessage("Email body", _testAdmin.Email))
            .Returns(new MailMsg("Email body", _testAdmin.Email));

        _emailServiceMock.Setup(e => e.SendInvitationEmailAsync(It.IsAny<MailMsg>()));

        // Act
        var result = await _adminService.InviteBusiness(invitationData, CancellationToken.None);

        // Assert
        Assert.True(result.Value is UserDtoToFrontEnd);
        Assert.Equal(_testVendorUser.Email, result.AsT0.Email);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<VendorUser>(), CancellationToken.None), Times.Once);
        _inviteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invite>(), CancellationToken.None), Times.Once);
        _emailServiceMock.Verify(e => e.SendInvitationEmailAsync(It.IsAny<MailMsg>()), Times.Once);
    }
    
    [Fact]
    public async Task InviteVendorUser_WhenBusinessIdIsZero_ReturnsError()
    {
        // Arrange
        var inviteDto = new DataForInviteDto { Email = "test@example.com", BusinessId = 0 };

        _adminRepositoryMock.Setup(r => r.GetByIdOrDefaultAsync( _testAdmin.Id))
            .ReturnsAsync(new Administrator { Id =  _testAdmin.Id });
        
        _userPrincipalServiceMock
            .SetupProperty(p => p.UserId, _testAdmin.Id);

        // Act
        var result = await _adminService.InviteVendorUser(inviteDto, CancellationToken.None);

        // Assert
        Assert.True(result.Value is Error);
    }
    
    [Fact]
    public async Task InviteVendorUser_WhenEmailIsEmpty_ReturnsError()
    {
        // Arrange
        var inviteDto = new DataForInviteDto { Email = "", BusinessId = 10 };

        _adminRepositoryMock.Setup(r => r.GetByIdOrDefaultAsync( _testAdmin.Id))
            .ReturnsAsync(new Administrator { Id =  _testAdmin.Id });
        
        _userPrincipalServiceMock
            .SetupProperty(p => p.UserId, _testAdmin.Id);

        // Act
        var result = await _adminService.InviteVendorUser(inviteDto, CancellationToken.None);

        // Assert
        Assert.True(result.Value is ValidationError);
    }
    


    private readonly Administrator _testAdmin = new Administrator
    {
        Id = 1,
        Email = "admin@example.com",
        FirstName = "Admin",
        LastName = "User"
    };

    private readonly OperatorUser _testOperatorUser = new OperatorUser
    {
        Id = 101,
        Email = "operator@example.com",
        OperatorId = 20
    };

    private readonly Invite _testInvite = new Invite
    {
        Id = 456,
        InvitedUserId = 101,
        SenderId = 1,
        CreatedAt = DateTime.UtcNow,
        Status = Sent
    };

    private readonly VendorUser _testVendorUser = new VendorUser
    {
        Id = 123,
        Email = "vendor@example.com",
        VendorId = 10,
        FirstName = "Invited",
        LastName = "User"
    };
}