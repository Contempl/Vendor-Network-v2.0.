using Moq;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Domain.Enum;
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
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IPasswordHasher> _passhwordHasherMock = new();
    

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
            _passhwordHasherMock.Object,
            _jwtTokenServiceMock.Object
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
            .Setup(r => r.GetByIdOrDefaultAsync( _testAdmin.Id))
            .ReturnsAsync(admin);

        _inviteServiceMock
            .Setup(s => s.CreateInviteByAdmin(It.IsAny<User>(), It.IsAny<Administrator>()))
            .Returns(invite);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(inviteDto.Email))
            .ReturnsAsync(createdUser);
        
        _emailServiceMock
            .Setup(e => e.CreateInviteUrl(It.IsAny<int>()))
            .Returns("https://invite.url/token");;

        // Act
        var result = await _adminService.InviteVendorUser( _testAdmin.Id, inviteDto);

        // Assert
        Assert.NotNull(result.Data);
        _inviteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invite>()), Times.Once);
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

        _adminRepositoryMock.Setup(r => r.GetByIdOrDefaultAsync( _testAdmin.Id))
            .ReturnsAsync(admin);

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(inviteDto.Email))
            .ReturnsAsync(createdUser);

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
        var result = await _adminService.InviteVendorUser( _testAdmin.Id, inviteDto);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(inviteDto.Email, result.Data.Email);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<VendorUser>()), Times.Once);
        _inviteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invite>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendInvitationEmailAsync(It.IsAny<MailMsg>()), Times.Once);
    }

    [Fact]
    public async Task InviteOperatorUser_ValidData_ReturnsUserDto()
    {
        // Arrange
        var adminId = 1;
        var inviteDto = new DataForInviteDto { Email = "operator@example.com", BusinessId = 20 };


        _adminRepositoryMock.Setup(r => r.GetByIdOrDefaultAsync(_testAdmin.Id))
            .ReturnsAsync(_testAdmin);

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(inviteDto.Email))
            .ReturnsAsync(_testOperatorUser);

        _inviteServiceMock
            .Setup(s => s.CreateInviteByAdmin(It.IsAny<User>(), It.IsAny<Administrator>()))
            .Returns(_testInvite);

        _emailServiceMock.Setup(e => e.CreateInviteUrl(_testInvite.Id))
            .Returns("https://invite.url/operator-token");

        _emailServiceMock.Setup(e =>
                e.GenerateEmailTemplate(inviteDto.Email, _testOperatorUser, "https://invite.url/operator-token"))
            .Returns("Operator Email Body");

        _emailServiceMock.Setup(e => e.CreateMessage("Operator Email Body", _testAdmin.Email))
            .Returns(new MailMsg("Operator Email Body", _testAdmin.Email));

        // Act
        var result = await _adminService.InviteOperatorUser(adminId, inviteDto);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(inviteDto.Email, result.Data.Email);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<OperatorUser>()), Times.Once);
        _inviteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invite>()), Times.Once);
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
            InvitedUserId = _testVenodrUser.Id,
            SenderId = _testAdmin.Id,
            Sender = _testAdmin,
            CreatedAt = DateTime.UtcNow,
            Status = Sent
        };

        _adminRepositoryMock.Setup(r => r.GetByIdOrDefaultAsync( _testAdmin.Id))
            .ReturnsAsync(_testAdmin);

         _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<VendorUser>()));

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(invitationData.UserEmail))
            .ReturnsAsync(_testVenodrUser);

        _inviteServiceMock.Setup(s => s.CreateInviteByAdmin(_testVenodrUser, _testAdmin))
            .Returns(invite);

        _inviteRepositoryMock.Setup(r => r.CreateAsync(invite));

        _emailServiceMock.Setup(e => e.CreateInviteUrl(invite.Id))
            .Returns("https://invite-link.com");
        
        _emailServiceMock
            .Setup(e => e.GenerateEmailTemplate(invitationData.UserEmail, _testVenodrUser, "https://invite-link.com"))
            .Returns("Email body");

        _emailServiceMock.Setup(e =>
                e.GenerateEmailTemplate(invitationData.BusinessEmail, _testVenodrUser, "https://invite-link.com"))
            .Returns("Email body");

        _emailServiceMock.Setup(e => e.CreateMessage("Email body", _testAdmin.Email))
            .Returns(new MailMsg("Email body", _testAdmin.Email));

        _emailServiceMock.Setup(e => e.SendInvitationEmailAsync(It.IsAny<MailMsg>()));

        // Act
        var result = await _adminService.InviteBusiness( _testAdmin.Id, invitationData);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(_testVenodrUser.Email, result.Data.Email);

        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<VendorUser>()), Times.Once);
        _inviteRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invite>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendInvitationEmailAsync(It.IsAny<MailMsg>()), Times.Once);
    }
    
    [Fact]
    public async Task InviteVendorUser_WhenBusinessIdIsZero_ReturnsError()
    {
        // Arrange
        var inviteDto = new DataForInviteDto { Email = "test@example.com", BusinessId = 0 };

        _adminRepositoryMock.Setup(r => r.GetByIdOrDefaultAsync( _testAdmin.Id))
            .ReturnsAsync(new Administrator { Id =  _testAdmin.Id });

        // Act
        var result = await _adminService.InviteVendorUser( _testAdmin.Id, inviteDto);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Data);
        Assert.Equal((int)ErrorCodes.InvalidInvitationData, result.ErrorCode);
        Assert.Equal("Invalid user data in invitation", result.ErrorMessage);
    }
    
    [Fact]
    public async Task InviteVendorUser_WhenAdminNotFound_ReturnsError()
    {
        // Arrange
        var adminId = 999;
        var inviteDto = new DataForInviteDto { Email = "test@example.com", BusinessId = 10 };

        _adminRepositoryMock.Setup(r => r.GetByIdAsync(adminId))
            .ReturnsAsync((Administrator)null!);

        // Act
        var result = await _adminService.InviteVendorUser(adminId, inviteDto);

        // Assert
        Assert.Equal((int)ErrorCodes.UserNotFound, result.ErrorCode);
        Assert.Equal("Admin not found.", result.ErrorMessage);
    }
    
    [Fact]
    public async Task InviteVendorUser_WhenEmailIsEmpty_ReturnsError()
    {
        // Arrange
        var inviteDto = new DataForInviteDto { Email = "", BusinessId = 10 };

        _adminRepositoryMock.Setup(r => r.GetByIdOrDefaultAsync( _testAdmin.Id))
            .ReturnsAsync(new Administrator { Id =  _testAdmin.Id });

        // Act
        var result = await _adminService.InviteVendorUser( _testAdmin.Id, inviteDto);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Data);
        Assert.Equal((int)ErrorCodes.InvalidInvitationData, result.ErrorCode);
        Assert.Equal("Invalid user data in invitation", result.ErrorMessage);
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

    private readonly VendorUser _testVenodrUser = new VendorUser
    {
        Id = 123,
        Email = "vendor@example.com",
        VendorId = 10,
        FirstName = "Invited",
        LastName = "User"
    };
}