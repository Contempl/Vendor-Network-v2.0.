using Moq;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Infrastructure.Implementations;
using Xunit;

namespace Product.Tests;

public class OperatorIndustryServiceTests
{
    private readonly Mock<IOperatorIndustryRepository> _operatorIndustryRepositoryMock = new ();
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new ();
    private readonly Mock<IOperatorRepository> _operatorRepositoryMock = new ();
    
    private readonly OperatorIndustryService _operatorIndustryService;

    public OperatorIndustryServiceTests()
    {
        _operatorIndustryService = new OperatorIndustryService(
            _operatorIndustryRepositoryMock.Object,
            _userPrincipalServiceMock.Object,
            _operatorRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetOperatorsIndustriesAsync_ReturnsListOfIndustriesSuccessfully()
    {
        // Arrange
        var industries = new List<OperatorIndustry>
        {
            new OperatorIndustry { Id = 1, Name = "Office", Address = "Test", Operator = _testOperator, OperatorId = 1},
            new OperatorIndustry { Id = 2, Name = "Office", Address = "Test", Operator = _testOperator, OperatorId = 1}
        };
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, 1);
        
        _operatorIndustryRepositoryMock.Setup(r => r.GetOperatorsIndustriesAsync(_testOperator.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(industries);
        
        // Act
        var result = await _operatorIndustryService.GetOperatorsIndustriesAsync( It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);        
    }
    
    [Fact]
    public async Task GetOperatorsIndustriesAsync_EmptyList_ReturnsErrorResponse()
    {
        // Arrange
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, 1);

        _operatorIndustryRepositoryMock.Setup(r => r.GetOperatorsIndustriesAsync(_testOperator.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OperatorIndustry>());
        
        // Act
        var result = await _operatorIndustryService.GetOperatorsIndustriesAsync(It.IsAny<CancellationToken>());

        // Assert
        Assert.Null(result.Data);
        Assert.Equal("Operator Industries Couldn't be fetched", result.ErrorMessage);
        Assert.Equal((int)ErrorCodes.InvalidOperatorIndustryData, result.ErrorCode);
    }


    [Fact]
    public async Task UpdateOperatorIndustryAsync_UpdatesOperatorIndustrySuccessfully()
    {
        // Arrange
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, 1);
        var operatorIndustry = new OperatorIndustry
        {
            Id = 1, Name = "Office", Address = "Test", Operator = _testOperator
        };
        var industrData = new UpdateOperatorIndustryDto
        {
            Address = "New address", Name = "New Name", Latitude = 10.0, Longitude = 10.0
        };
        
        OperatorIndustry updatedIndustry = null;

        _operatorIndustryRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<OperatorIndustry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask); 


        _operatorIndustryRepositoryMock.Setup(r =>
            r.GetByIdAsync(_testOperator.Id, operatorIndustry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(operatorIndustry);

        // Act
        var result = await _operatorIndustryService
            .UpdateOperatorIndustryAsync(operatorIndustry.Id, industrData, It.IsAny<CancellationToken>());

        // Assert
        Assert.Equal(industrData.Address, result.Data.Address);
        Assert.Equal(industrData.Name, result.Data.Name);
        Assert.Equal(industrData.Name, result.Data.Name);
        Assert.Equal(industrData.Address, result.Data.Address);
        _operatorIndustryRepositoryMock.Verify(r => 
            r.GetByIdAsync(_testOperator.Id, operatorIndustry.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        _operatorIndustryRepositoryMock.Verify(r => 
            r.UpdateAsync(operatorIndustry, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveOperatorIndustryAsync_WithExistingOperatorIndustry_DeletesSuccessfully()
    {
        // Arrange
        var operatorIndustry = new OperatorIndustry
        {
            Id = 1, Name = "Office", Address = "Test", Operator = _testOperator
        };

        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, 1);
        _operatorIndustryRepositoryMock.Setup(r => 
            r.GetByIdAsync(_testOperator.Id, operatorIndustry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(operatorIndustry);
        
        // Act
        var result = await _operatorIndustryService.RemoveOperatorIndustryAsync(operatorIndustry.Id, It.IsAny<CancellationToken>());
        
        // Assert
        Assert.Equal(operatorIndustry.Id, result.Data);
        _operatorIndustryRepositoryMock.Verify(r =>
            r.DeleteAsync(It.Is<OperatorIndustry>(oi => oi.Id == operatorIndustry.Id), It.IsAny<CancellationToken>()), Times.Once);
        
        _operatorIndustryRepositoryMock.Verify(r => 
            r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddOperatorIndustryAsync_CreateOperatorIndustry_CreatesIndustrySuccessfully()
    {
        // Arrange
        var industryCreationDto = new OperatorIndustryCreationDto
        {
            Name = "Office", Address = "Test", Longitude = 100.0, Latitude = 100.0,
        };
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, _testOperator.Id);
        _operatorRepositoryMock.Setup(r => 
            r.GetByIdAsync(_testOperator.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_testOperator);
        
        _operatorIndustryRepositoryMock.Setup(r => 
            r.CreateAsync(It.IsAny<OperatorIndustry>(), It.IsAny<CancellationToken>()));
        
        // Act
        var result = await _operatorIndustryService.CreateOperatorIndustryAsync(industryCreationDto, It.IsAny<CancellationToken>());
        
        // Assert
        Assert.Equal(industryCreationDto.Name, result.Data.Name);
        Assert.Equal(industryCreationDto.Address, result.Data.Address);
        _operatorIndustryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<OperatorIndustry>(), It.IsAny<CancellationToken>()), Times.Once);
        _operatorRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOpIndustryByIdAsync_FetchesIndustrySuccessfully()
    {
        // Arrange
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, 1);
        var operatorIndustry = new OperatorIndustry
        {
            Id = 1, Name = "Office", Address = "Test", Operator = _testOperator
        };
        _operatorIndustryRepositoryMock
            .Setup(r => r.GetByIdAsync(_testOperator.Id, operatorIndustry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorIndustry);
        
        // Act
        var result = await _operatorIndustryService.GetOpIndustryByIdAsync(operatorIndustry.Id, It.IsAny<CancellationToken>());
        
        // Assert
        Assert.Equal(operatorIndustry.Address, result.Data.Address);
        Assert.Equal(operatorIndustry.Name, result.Data.Name);
        _operatorIndustryRepositoryMock.Verify(r => 
            r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private readonly Operator _testOperator = new Operator()
    {
        BusinessName = "test", Address = "test",
        Email = "test@test.com", Id = 1, Occupation = "test",
    };
}