using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;
using Product.Infrastructure.Implementations;
using Xunit;

namespace Product.Tests;

public class VendorFacilityServiceTests
{

    private readonly Mock<IVendorRepository> _vendorRepositoryMock = new();
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new();
    private readonly Mock<IVendorFacilityRepository> _vendorFacilityRepositoryMock = new();
    private readonly Mock<ILogger<VendFacilityService>> _loggerMock = new();
    
    private readonly VendFacilityService _vendorFacilityService; 

    public VendorFacilityServiceTests()
    {
        _vendorFacilityService = new VendFacilityService(
            _vendorFacilityRepositoryMock.Object,
            _userPrincipalServiceMock.Object,
            _vendorRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetFacilityWithServicesByIdAsync_ReturnsCorrectFacilityWithServices()
    {
        // Arrange
        var facilityId = 1;
        var vendorFacility = new VendorFacility
        {
            Id = 1, Name = "Test Name", Services = _testVendorFacilityServices,
        };

        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, _testVendor.Id);
        _vendorFacilityRepositoryMock.Setup(r => 
                r.GetFacilityWithServicesByIdAsync(facilityId, _testVendor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendorFacility);
        
        // Act
        var result = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId, It.IsAny<CancellationToken>());
        
        // Assert
        Assert.True(result.Value is VendorFacility);
        var facility = result.Value as VendorFacility;
        Assert.Equal(4, facility.Services.Count);
        Assert.Equal("Service Name #1", facility.Services[0].Name);
    }
    
    [Fact]
    public async Task AddFacilityAsync_CreatesNewFacility()
    {
        // Arrange
        var vendorId = _testVendor.Id;
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, vendorId);
        var creationDto = new VendorFacilityDto
        {
            Name = "Test Name", Location = "Test Location", Longitude = 10, Latitude = 20, RadiusOfWork = 40000,
            Services = {"Service Name #1", "Service Name #2", "Service Name #3", "Service Name #4"},
        };
        
        _vendorRepositoryMock.Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testVendor);
        
        // Act
        var result = await _vendorFacilityService.AddFacilityAsync(creationDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is VendorFacility);
        var facility = result.Value as VendorFacility;
        Assert.Equal("Test Name", facility!.Name);
        _vendorFacilityRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<VendorFacility>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AddFacilityAsync_WithNullServices_ReturnsCorrectErrorResponse()
    {
        // Arrange
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, _testVendor.Id);
        var creationDto = new VendorFacilityDto
        {
            Name = "Test Name", Location = "Test Location", Longitude = 10, Latitude = 20, RadiusOfWork = 40000,
        };
        _vendorRepositoryMock.Setup(r => r.GetByIdAsync(_testVendor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testVendor);
        
        // Act
        var result = await _vendorFacilityService.AddFacilityAsync(creationDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is NotFoundError);
    }
    
    [Fact]
    public async Task UpdateFacilityAsync_UpdatesVendorFacilities()
    {
        // Arrange
        var vendFacility = _testVendorFacility;
        vendFacility.Services = _testVendorFacilityServices;
        var updateDto = new UpdateVendorFacilityDto
        {
            Name = "New Name", Location = "New Location",
        };
        
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, _testVendor.Id);
        
        _vendorFacilityRepositoryMock.Setup(r => 
            r.GetFacilityWithServicesByIdAsync(_testVendor.Id, _testVendorFacility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testVendorFacility);
        
        
        // Act
        var result =  await _vendorFacilityService
            .UpdateFacilityAsync(vendFacility.Id, updateDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is VendorFacility);
        _vendorFacilityRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<VendorFacility>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateFacilityAsync_UpdatesVendorFacilityServices()
    {
        // Arrange
        var vendFacility = _testVendorFacility;
        vendFacility.Services = _testVendorFacilityServices;
        var updateDto = new UpdateVendorFacilityDto
        {
            Services = { "New Service Name #1, New Service Name #2, New Service Name #3" },
        };
        
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, _testVendor.Id);
        
        _vendorFacilityRepositoryMock.Setup(r => 
                r.GetFacilityWithServicesByIdAsync(_testVendor.Id, _testVendorFacility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testVendorFacility);
        
        // Act
        var result =  await _vendorFacilityService
            .UpdateFacilityAsync(vendFacility.Id, updateDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is VendorFacility);
        var facility = result.Value as VendorFacility;
        _vendorFacilityRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<VendorFacility>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(updateDto.Services[0], facility!.Services[0].Name);
    }
    
    
    [Fact]
    public async Task DeleteFacilityAsync_RemovesFacilitySuccessfully()
    {
        // Arrange
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, _testVendor.Id);
        _vendorFacilityRepositoryMock.Setup(r => 
                r.GetByIdAsync(_testVendor.Id, _testVendorFacility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testVendorFacility);
        
        // Act
        var result = await _vendorFacilityService.RemoveFacilityAsync(_testVendor.Id, _testVendorFacility.Id, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is int);
        var facility = (int)result.Value;
        Assert.Equal(_testVendorFacility.Id, facility);
        _vendorFacilityRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<VendorFacility>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetVendorFacilityServiceAsync_ReturnsCorrectErrorCode_WhenFacilityServiceNotFound()
    {
        // Arrange
        var vendorFacility = _testVendorFacility;
        vendorFacility.Services = _testVendorFacilityServices;
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, _testVendor.Id);
        _vendorFacilityRepositoryMock.Setup(r => 
            r.GetFacilityWithServicesByIdAsync(_testVendor.Id, _testVendorFacility.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendorFacility);
        
        // Act
        var result = await _vendorFacilityService
            .GetVendorFacilityServiceAsync(_testVendorFacility.Id, 16, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is NotFoundError);
    }
    
    private readonly Vendor _testVendor = new Vendor()
    {
        Id = 1,
        BusinessName = "Test Name",
        Address = "Test Address",
        Email = "test@test.com",
    };

    private readonly VendorFacility _testVendorFacility = new VendorFacility()
    {
        Id = 1, Name = "Test Name", Longitude = 100, Latitude = 100, Location = "Test Location", RadiusOfWork = 40000,
    };

    private readonly List<VendorFacilityService> _testVendorFacilityServices = new()
    {
        new VendorFacilityService() { Id = 1, Name = "Service Name #1", },
        new VendorFacilityService() { Id = 2, Name = "Service Name #2", },
        new VendorFacilityService() { Id = 3, Name = "Service Name #3", },
        new VendorFacilityService() { Id = 4, Name = "Service Name #4", },
    };
}