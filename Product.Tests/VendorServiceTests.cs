using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Types;
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

public class VendorServiceTests
{
    private readonly Mock<IVendorRepository> _vendorRepositoryMock = new();
    private readonly Mock<IOperatorRepository> _operatorRepositoryMock = new();
    private readonly Mock<IVendorUserRepository> _vendorUserRepositoryMock = new();
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IInviteService> _inviteServiceMock = new();
    private readonly Mock<IUserRepository > _userRepositoryMock = new();
    private readonly Mock<IInviteRepository> _inviteRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<VendorService>> _loggerMock = new();
    
    private readonly VendorService _vendorService; 

    public VendorServiceTests()
    {
        _vendorService = new VendorService(
            _vendorRepositoryMock.Object,
            _operatorRepositoryMock.Object,
            _vendorUserRepositoryMock.Object,
            _userPrincipalServiceMock.Object,
            _emailServiceMock.Object,
            _inviteServiceMock.Object,
            _inviteRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
            );
    }
    
    [Fact]
    public async Task SearchOperatorsAsync_ReturnsCorrectErrorResponse()
    {
        // Arrange
        var operatorSearchDto = new OperatorSearchDto { Name = "" };
        
        // Act
        var result = await _vendorService.SearchOperatorsAsync(operatorSearchDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is InvalidOperatorNameError);
    }
    
    [Fact]
    public async Task GetVendorByIdAsync_ReturnsVendor()
    {
        // Arrange
        _vendorRepositoryMock.Setup(r => r.GetByIdAsync(_testVendor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testVendor);
        
        // Act
        var result = await _vendorService.GetVendorByIdAsync(_testVendorUser.Id, It.IsAny<CancellationToken>());
        
        // Assert
        Assert.True(result.Value is BusinessFrontEndDto);
        var resultDto = result.Value as BusinessFrontEndDto;
        Assert.Equal(_testVendor.BusinessName, resultDto!.BusinessName);
        Assert.Equal(_testVendor.Address, resultDto.Address);
        _vendorRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateVendorAsync_UpdatesVendorSuccessfully()
    {
        // Arrange
        var updateVendorDto = new UpdateVendorDto
        {
            BusinessName = "Business Name", Address = "Test Address", Email = "test@test.com",
        };
        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, 1);
        _vendorRepositoryMock.Setup(r => r.GetByIdAsync(_testVendor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testVendor);

        // Act
        var result = await _vendorService.UpdateVendorAsync(updateVendorDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is BusinessFrontEndDto);
        var resultDto = result.Value as BusinessFrontEndDto;
        Assert.Equal(updateVendorDto.BusinessName, resultDto!.BusinessName);
        Assert.Equal(updateVendorDto.Address, resultDto.Address);
        _vendorRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Vendor>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task InviteVendorUserAsync_ReturnsMailMessage()
    {
        // Arrange
        var emailDto = new EmailForInviteDto { Email = "test@test.com" };
        var newVendorUser = new VendorUser {Id = 11, Email = emailDto.Email, VendorId = _testVendor.Id };

        var mockInvite = new Invite
        {
            Id = 12, 
            InvitedUserId = newVendorUser.Id,
            InvitedUser = newVendorUser,
            SenderId = _testVendorUser.Id,
            Status = InvitationStatus.Sent,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
        };
        

        var transactionMock = new Mock<IDbContextTransaction>();
        
        _userPrincipalServiceMock.SetupProperty(r => r.UserId, _testVendorUser.Id);
        
        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, _testVendor.Id);
        
        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);;
        
        _vendorRepositoryMock.Setup(r => r.GetByIdAsync(_testVendor.Id, CancellationToken.None))
            .ReturnsAsync(_testVendor);
        
        _vendorUserRepositoryMock.Setup(r => r.GetByIdAsync(_testVendorUser.Id, CancellationToken.None))
            .ReturnsAsync(_testVendorUser);
        
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(emailDto.Email, CancellationToken.None))
            .ReturnsAsync(newVendorUser);

        _inviteServiceMock.Setup(s => s.CreateInvite(It.IsAny<VendorUser>(), It.IsAny<VendorUser>()))
            .Returns(mockInvite);
        
        _emailServiceMock.Setup(s => s.CreateInviteUrl(It.IsAny<int>()))
            .Returns("http://test.com/invite/123");

        _emailServiceMock.Setup(s => s.GenerateEmailTemplate(emailDto.Email, _testVendorUser, "test.com"))
            .Returns("Test.Template.");
        
        _emailServiceMock.Setup(s => s.CreateMessage(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new MailMsg(It.IsAny<string>(), It.IsAny<string>()));

        _inviteRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Invite>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        
        // Act
        var result = await _vendorService.InviteVendorUserAsync(emailDto, It.IsAny<CancellationToken>());
        
        // Assert
        Assert.True(result.Value is MailMsg);
        _vendorUserRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<VendorUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(s => s.CreateMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _emailServiceMock.Verify(s => s.SendInvitationEmailAsync(It.IsAny<MailMsg>()), Times.Once);
    }

    private readonly VendorUser _testVendorUser = new VendorUser
    {
          Id = 1, UserName = "test", Email = "test@test.com", FirstName = "test", LastName = "test", UserType = UserType.VendorUser
    };
    
    private readonly Vendor _testVendor = new Vendor()
    {
        Id = 1,
        BusinessName = "Test Name",
        Address = "Test Address",
        Email = "test@test.com",
    };
}