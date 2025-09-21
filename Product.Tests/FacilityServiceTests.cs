using System.Threading.Tasks;
using Moq;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Infrastructure.Implementations;
using Xunit;

namespace Product.Tests;

public class FacilityServiceTests
{
    private readonly Mock<IVendorFacilityServiceRepository> _vendorFacilityServiceRepositoryMock = new();
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new();
    private readonly Mock<IVendorFacilityRepository> _vendorFacilityRepositoryMock = new();

    private readonly FacilityService _facilityService;

    public FacilityServiceTests()
    {
        _facilityService = new FacilityService(
            _vendorFacilityServiceRepositoryMock.Object,
            _userPrincipalServiceMock.Object,
            _vendorFacilityRepositoryMock.Object
        );
    }
    
    [Fact]
    public async Task AddFacilityServiceAsync_ValidData_ReturnsResponse()
    {
        // Arrange
        string serviceName = "Cleaning";

        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, 1);
        _vendorFacilityRepositoryMock.Setup(r => r.GetByIdAsync(_testVendorFacility.Id, _testVendor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testVendorFacility);

        // Act
        var result = await _facilityService.AddFacilityServiceAsync(_testVendorFacility.Id, serviceName, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(serviceName, result.Data.Name);
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task AddFacilityServiceAsync_ValidData_ReturnsCreatedService()
    {
        // Arrange
        string serviceName = "Cleaning";
        
        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, 1);
        _vendorFacilityRepositoryMock.Setup(r => r.GetByIdAsync(_testVendorFacility.Id, _testVendor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testVendorFacility);
        
        // Act
        var result = await _facilityService.AddFacilityServiceAsync(_testVendorFacility.Id, serviceName, It.IsAny<CancellationToken>());

        // Assert
        var createdService = result.Data;
        Assert.NotNull(createdService);
        Assert.Equal(serviceName, createdService.Name);
        Assert.Equal(_testVendorFacility.Id, createdService.VendorFacilityId);
    }
    
    [Fact]
    public async Task UpdateFacilityServiceAsync_ValidData_ReturnsUpdatedService()
    {
        // Arrange
        int vendorId = 1;
        int facilityId = 1;
        int serviceId = 10;
        var existingService = new VendorFacilityService { Id = serviceId, VendorFacilityId = facilityId, Name = "Old Name" };
        var updatedDto = new VendorFacilityServiceDto { Name = "New Name" };

        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, 1);
        _vendorFacilityServiceRepositoryMock
            .Setup(r => r.GetByIdAsync(vendorId, facilityId, serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingService);

        _vendorFacilityServiceRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<VendorFacilityService>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _facilityService.UpdateFacilityServiceAsync(facilityId, serviceId, updatedDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(updatedDto.Name, result.Data.Name);
        _vendorFacilityServiceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<VendorFacilityService>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFacilityServiceAsync_ValidId_ReturnsRemovedServiceId()
    {
        // Arrange
        int vendorId = 1;
        int facilityId = 1;
        int serviceId = 10;
        var existingService = new VendorFacilityService { Id = serviceId, VendorFacilityId = facilityId };
        
        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, 1);
        _vendorFacilityServiceRepositoryMock
            .Setup(r => r.GetByIdAsync(vendorId, facilityId, serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingService);

        _vendorFacilityServiceRepositoryMock
            .Setup(r => r.DeleteAsync(existingService, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _facilityService.RemoveFacilityServiceAsync(facilityId, serviceId, It.IsAny<CancellationToken>());

        // Assert
        Assert.Equal(serviceId, result.Data);
        _vendorFacilityServiceRepositoryMock.Verify(r => r.DeleteAsync(existingService, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddFacilityServiceAsync_InvalidData_ReturnsError()
    {
        // Arrange
        int facilityId = 0;
        string serviceName = "";
        
        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, 1);

        // Act
        var result = await _facilityService.AddFacilityServiceAsync(facilityId, serviceName, It.IsAny<CancellationToken>());

        // Assert
        Assert.Null(result.Data);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateFacilityServiceAsync_ServiceNotFound_ReturnsError()
    {
        // Arrange
        int vendorId = 1;
        int facilityId = 1;
        int serviceId = 999;
        var updatedDto = new VendorFacilityServiceDto { Name = "New Name" };

        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, 1);
        _vendorFacilityServiceRepositoryMock
            .Setup(r => r.GetByIdAsync(vendorId, facilityId, serviceId, It.IsAny<CancellationToken>()))!
            .ReturnsAsync((VendorFacilityService)null!);

        // Act
        var result = await _facilityService.UpdateFacilityServiceAsync(facilityId, serviceId, updatedDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.Null(result.Data);
        Assert.NotNull(result.ErrorMessage);
    }
    
    [Fact]
    public async Task RemoveFacilityServiceAsync_ServiceNotFound_ReturnsError()
    {
        // Arrange
        int vendorId = 1;
        int facilityId = 1;
        int serviceId = 999;

        _userPrincipalServiceMock.SetupProperty(r => r.BusinessId, 1);
        _vendorFacilityServiceRepositoryMock
            .Setup(r => r.GetByIdAsync(vendorId, facilityId, serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VendorFacilityService)null!);

        // Act
        var result = await _facilityService.RemoveFacilityServiceAsync(facilityId, serviceId, It.IsAny<CancellationToken>());

        // Assert
        Assert.Equal(0, result.Data);
        Assert.NotNull(result.ErrorMessage);
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
}