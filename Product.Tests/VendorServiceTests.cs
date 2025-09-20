using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
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
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object
            );
    }
    
    [Fact]
    public async Task SearchOperatorsAsync_ReturnsCorrectErrorResponse()
    {
        // Arrange
        var operatorSearchDto = new OperatorSearchDto { Name = "" };
        
        // Act
        var result = await _vendorService.SearchOperatorsAsync(operatorSearchDto);

        // Assert
        Assert.Equal((int)ErrorCodes.InvalidBusinessName, result.ErrorCode);
        Assert.Equal("Invalid Operator Name", result.ErrorMessage);
    }
    
    [Fact]
    public async Task GetVendorByIdAsync_ReturnsVendor()
    {
        // Arrange
        _vendorRepositoryMock.Setup(r => r.GetByIdAsync(_testVendor.Id))
            .ReturnsAsync(_testVendor);
        
        // Act
        var result = await _vendorService.GetVendorByIdAsync(_testVendorUser.Id);
        
        // Assert
        Assert.Equal(_testVendor.BusinessName, result.Data.BusinessName);
        Assert.Equal(_testVendor.Address, result.Data.Address);
        _vendorRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
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
        _vendorRepositoryMock.Setup(r => r.GetByIdAsync(_testVendor.Id))
            .ReturnsAsync(_testVendor);

        // Act
        var result = await _vendorService.UpdateVendorAsync(updateVendorDto);

        // Assert
        Assert.Equal(updateVendorDto.BusinessName, result.Data.BusinessName);
        Assert.Equal(updateVendorDto.Address, result.Data.Address);
        _vendorRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Vendor>()), Times.Once);
    }
    
    [Fact]
    public async Task InviteVendorUserAsync_CallsAllMethodsWithWhenParametersAreValid()
    {
        // Arrange
        var emailDto = new EmailForInviteDto { Email = "test@test.com" };
        var newVendorUser = new VendorUser { Email = emailDto.Email, VendorId = _testVendor.Id };
        var transactionMock = new Mock<IDbContextTransaction>();
        
        _userPrincipalServiceMock.SetupProperty(r => r.UserId, _testVendorUser.Id);
        
        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, _testVendorUser.Id);
        
        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);;
        
        _vendorRepositoryMock.Setup(r => r.GetByIdAsync(_testVendor.Id))
            .ReturnsAsync(_testVendor);
        
        _vendorUserRepositoryMock.Setup(r => r.GetByIdAsync(_testVendorUser.Id))
            .ReturnsAsync(_testVendorUser);
        
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(emailDto.Email))
            .ReturnsAsync(newVendorUser);

        _inviteServiceMock.Setup(s => s.CreateInvite(It.IsAny<User>(), It.IsAny<User>()))
            .Returns(new Invite());
        
        // Act
        var result = await _vendorService.InviteVendorUserAsync(emailDto);
        
        // Assert
        _vendorUserRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<VendorUser>()), Times.Once);
        _emailServiceMock.Verify(s => s.CreateMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _emailServiceMock.Verify(s => s.SendInvitationEmailAsync(It.IsAny<MailMsg>()), Times.Once);
    }

    private readonly VendorUser _testVendorUser = new VendorUser
    {
          Id = 1, UserName = "test", Email = "test@test.com", FirstName = "test", LastName = "test"
    };
    
    private readonly Vendor _testVendor = new Vendor()
    {
        Id = 1,
        BusinessName = "Test Name",
        Address = "Test Address",
        Email = "test@test.com",
    };
}