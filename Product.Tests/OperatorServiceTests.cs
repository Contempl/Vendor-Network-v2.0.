using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Moq;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Pagination;
using Product.Infrastructure.Implementations;
using Xunit;

namespace Product.Tests;

public class OperatorServiceTests
{
    private readonly Mock<IOperatorRepository> _operatorRepositoryMock = new ();
    private readonly Mock<IVendorRepository> _vendorRepositoryMock = new ();
    private readonly Mock<IOperatorIndustryRepository> _operatorFacilityRepositoryMock = new ();
    private readonly Mock<IOperatorUserRepository> _operatorUserRepositoryMock = new ();
    private readonly Mock<IEmailService> _emailServiceMock = new ();
    private readonly Mock<IInviteRepository> _inviteRepositoryMock = new ();
    private readonly Mock<IUserRepository> _userRepositoryMock = new ();
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new();
    private readonly Mock<IInviteService> _inviteServiceMock = new();
    
    private readonly OperatorService _operatorService;

    public OperatorServiceTests()
    {
        _operatorService = new OperatorService(
            _operatorRepositoryMock.Object,
            _vendorRepositoryMock.Object,
            _operatorFacilityRepositoryMock.Object,
            _userRepositoryMock.Object,
            _operatorUserRepositoryMock.Object,
            _emailServiceMock.Object,
            _inviteRepositoryMock.Object,
            _userPrincipalServiceMock.Object,
            _inviteServiceMock.Object
        );
    }
    
    [Fact]
    public async Task GetOperatorAsync_ValidId_ReturnsOperator()
    {
        // Arrange
        var operatorId = 1;
        var operatorEntity = new Operator
        {
            Id = operatorId,
            BusinessName = "Test Operator"
        };

        _operatorRepositoryMock.Setup(r => r.GetByIdAsync(operatorId))
            .ReturnsAsync(operatorEntity);

        // Act
        var result = await _operatorService.GetOperatorAsync(operatorId);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal("Test Operator", result.Data.BusinessName);
        _operatorRepositoryMock.Verify(r => r.GetByIdAsync(operatorId), Times.Once);
    }

    [Fact]
    public async Task RegisterOperatorAsync_ValidData_ReturnsOperator()
    {
        // Arrange
        var operatorId = 1;
        var operatorRegData = new OperatorRegistrationDto
        {
            BusinessName = "Test Operator",
            Address = "Test Address",
            Email = "test@test.com",
            Occupation = "Test Occupation",
        };

        _operatorUserRepositoryMock.Setup(r => r.GetByIdAsync(operatorId))
            .ReturnsAsync(_testOperatorUser);
        
        // Act
        var result = await _operatorService.RegisterOperatorAsync(operatorId, operatorRegData);
        
        // Assert
        Assert.Equal("Test Operator", result.Data.BusinessName);
        Assert.Equal("Test Address", result.Data.Address);
    }

    [Fact]
    public async Task InviteOperatorUserAsync_ValidData_ReturnsMailMessage()
    {
        // Arrange
        var operatorUserId = 1;
        var businessId = 1;
        var emailDto = new EmailForInviteDto { Email = "test@test.com" };
        var newOperatorUser = new OperatorUser { Email = emailDto.Email, OperatorId = businessId };
        var invite = new Invite { Id = 33 };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(operatorUserId)).ReturnsAsync(_testOperatorUser);
        _userPrincipalServiceMock.SetupProperty(p => p.BusinessId, businessId);
        _operatorUserRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<OperatorUser>()));

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(emailDto.Email))
            .ReturnsAsync(newOperatorUser);
        _inviteServiceMock.Setup(s =>  s.CreateInvite(newOperatorUser, _testOperatorUser))
            .Returns(invite);
        _emailServiceMock.Setup(s => s.CreateInviteUrl(invite.Id))
            .Returns("https://invite.url/operator-token");
        _inviteRepositoryMock.Setup(r => r.CreateAsync(invite));
        _emailServiceMock.Setup(s => 
                s.GenerateEmailTemplate(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>()))
            .Returns("Operator Email Body");

        _emailServiceMock.Setup(s =>
                s.CreateMessage(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string body, string sender) => new MailMsg(body, sender));

        _emailServiceMock.Setup(e => e.SendInvitationEmailAsync(It.IsAny<MailMsg>()))
            .Returns(Task.CompletedTask);


        
        // Act
        var result = await _operatorService.InviteOperatorUserAsync(operatorUserId, emailDto);
        
        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal("Operator Email Body", result.Data.Body);
        Assert.Equal(_testOperatorUser.Email, result.Data.Sender);
    }
    
    [Fact]
    public async Task UpdateOperatorAsync_ValidData_UpdatesAndReturnsOperatorDto()
    {
        // Arrange
        var operatorId = 100;
        var updateDto = new UpdateOperatorDto
        {
            BusinessName = "Updated Operator",
            Address = "Updated Address"
        };

        var existingOperator = new Operator
        {
            Id = operatorId,
            BusinessName = "Old Name",
            Occupation = "Old Address"
        };

        _operatorRepositoryMock.Setup(r => r.GetByIdAsync(operatorId))
            .ReturnsAsync(existingOperator);

        _operatorRepositoryMock.Setup(r => r.UpdateAsync(existingOperator))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _operatorService.UpdateOperatorAsync(operatorId, updateDto);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(updateDto.BusinessName, result.Data.BusinessName);
        Assert.Equal(updateDto.Address, result.Data.Address);
    }
    
    [Fact]
    public async Task RemoveOperatorAsync_ValidId_RemovesOperatorAndReturnsSuccess()
    {
        // Arrange
        var operatorId = 100;
        var existingOperator = new Operator { Id = operatorId };

        _operatorRepositoryMock.Setup(r => r.GetByIdAsync(operatorId))
            .ReturnsAsync(existingOperator);

        _operatorRepositoryMock.Setup(r => r.DeleteAsync(existingOperator));

        // Act
        var result = await _operatorService.RemoveOperatorAsync(operatorId);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(operatorId, result.Data);
    }
    
    [Fact]
    public async Task GetOperatorAsync_ValidId_ReturnsOperatorDto()
    {
        // Arrange
        var operatorId = 100;
        var existingOperator = new Operator
        {
            Id = operatorId,
            BusinessName = "Test Operator"
        };

        _operatorRepositoryMock.Setup(r => r.GetByIdAsync(operatorId))
            .ReturnsAsync(existingOperator);

        // Act
        var result = await _operatorService.GetOperatorAsync(operatorId);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(existingOperator.Id, result.Data.Id);
        Assert.Equal(existingOperator.BusinessName, result.Data.BusinessName);
    }

    [Fact]
    public async Task SearchForVendorsAsync_ValidIndustries_ReturnsVendorList()
    {
        // Arrange
        var existingOperator = new Operator
        {
            Id = 100,
            BusinessName = "Test Operator"
        };
        var industriesData = new SearchVendorsForIndustriesDto
        {
            ServiceType = "SomeService",
            IndustriesLocationIds = new List<int> { 1, 2 }
        };
        var vendorFacilities = new List<VendorFacility>
        {
            new VendorFacility
            {
                Id = 1,
                Services = new List<VendorFacilityService>
                {
                    new VendorFacilityService { Name = "SomeService" }
                }
            }
        };
        
        var vendors = new List<Vendor>
        {
            new Vendor { Id = 1, BusinessName = "CleanCo", VendorFacilities = vendorFacilities },
            new Vendor { Id = 2, BusinessName = "Sparkle Services", VendorFacilities = vendorFacilities  }
        };
    
        var operatorIndustries = new List<OperatorIndustry>
        {
            new OperatorIndustry { Id = 1, Name = "Office", Address = "Test", Operator = existingOperator},
            new OperatorIndustry { Id = 2, Name = "Retail", Address = "TEst", Operator = existingOperator}
        }.AsQueryable();

        _vendorRepositoryMock
            .Setup(r => r.GetVendorsWithService("SomeService"))
            .ReturnsAsync(vendors);
        _operatorFacilityRepositoryMock
            .Setup(r => r.GetAll())
            .Returns(operatorIndustries);
    


        _vendorRepositoryMock.Setup(r => r.GetVendorsWithService("CleanCo")).ReturnsAsync(vendors);
;
    
        // Act
        var result = await _operatorService.SearchForVendorsAsync(industriesData);
    
        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("CleanCo", result.Data[0].BusinessName);
        Assert.Equal("Sparkle Services", result.Data[1].BusinessName);
    }
    
    [Fact]
    public async Task GetVendorsByNameAsync_ValidSearch_ReturnsPagedVendors()
    {
        // Arrange
        var searchDto = new VendorSearchDto
        {
            VendorName = "Clean",
            PageNumber = 1,
            PageSize = 10
        };
        
        var pagedResult = new PagedResult<Vendor>
        {
            Items =
            [
                new Vendor { Id = 1, BusinessName = "CleanCo" },
                new Vendor { Id = 2, BusinessName = "CleanPro" }
            ],
            TotalCount = 2
        };

        _vendorRepositoryMock.Setup(r => r.GetVendorsQuery(searchDto.VendorName, SortOrder.Ascending,  searchDto.PageSize, searchDto.PageNumber ))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _operatorService.GetVendorsByNameAsync(searchDto);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.Equal("CleanCo", result.Data.Items[0].BusinessName);
        Assert.Equal("CleanPro", result.Data.Items[1].BusinessName);
        Assert.Equal(searchDto.PageNumber, result.Data.PageNumber);
        Assert.Equal(searchDto.PageSize, result.Data.PageSize);
    }
    
    [Fact]
    public async Task SearchForVendorsAsync_EmptyServiceType_ReturnsErrorResponse()
    {
        // Arrange
        var industriesData = new SearchVendorsForIndustriesDto
        {
            ServiceType = "",
            IndustriesLocationIds = new List<int> { 1, 2 }
        };

        // Act
        var result = await _operatorService.SearchForVendorsAsync(industriesData);

        // Assert
        Assert.Null(result.Data);
        Assert.Equal((int)ErrorCodes.InvalidServiceType, result.ErrorCode);
        Assert.Contains(nameof(industriesData.ServiceType), result.ErrorMessage);
    }
    

    private readonly OperatorUser _testOperatorUser = new OperatorUser
    {
        Id = 1,
        Email = "test@test.com",
        FirstName = "test",
    };
}