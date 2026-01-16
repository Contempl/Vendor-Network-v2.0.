using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Types;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Infrastructure.Implementations;
using Xunit;

namespace Product.Tests;

public class OperatorIndustryServiceTests
{
    private readonly Mock<IOperatorIndustryRepository> _operatorIndustryRepositoryMock = new ();
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new ();
    private readonly Mock<IOperatorRepository> _operatorRepositoryMock = new ();
    private readonly Mock<ILogger<OperatorIndustryService>> _loggerMock = new ();
    
    private readonly OperatorIndustryService _operatorIndustryService;

    public OperatorIndustryServiceTests()
    {
        _operatorIndustryService = new OperatorIndustryService(
            _operatorIndustryRepositoryMock.Object,
            _userPrincipalServiceMock.Object,
            _operatorRepositoryMock.Object,
            _loggerMock.Object
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
        
        _operatorIndustryRepositoryMock.Setup(r => r.GetOperatorsIndustriesAsync(_testOperator.Id, CancellationToken.None))
            .ReturnsAsync(industries);
        
        // Act
        var result = await _operatorIndustryService.GetOperatorsIndustriesAsync( CancellationToken.None);

        // Assert
        Assert.True(result.Value is List<OpIndustryFrontEndDto>);
        var industriesFrontEnd = result.Value as List<OpIndustryFrontEndDto>;
        Assert.Equal(2, industriesFrontEnd!.Count);        
    }
    
    [Fact]
    public async Task GetOperatorsIndustriesAsync_EmptyList_ReturnsErrorResponse()
    {
        // Arrange
        _userPrincipalServiceMock.SetupProperty(s => s.BusinessId, 1);

        _operatorIndustryRepositoryMock.Setup(r => r.GetOperatorsIndustriesAsync(_testOperator.Id, CancellationToken.None))
            .ReturnsAsync(new List<OperatorIndustry>());
        
        // Act
        var result = await _operatorIndustryService.GetOperatorsIndustriesAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Value is Error);
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
            .Setup(r => r.UpdateAsync(It.IsAny<OperatorIndustry>(), CancellationToken.None))
            .Returns(Task.CompletedTask); 


        _operatorIndustryRepositoryMock.Setup(r =>
            r.GetByIdAsync(_testOperator.Id, operatorIndustry.Id, CancellationToken.None)).ReturnsAsync(operatorIndustry);

        // Act
        var result = await _operatorIndustryService
            .UpdateOperatorIndustryAsync(operatorIndustry.Id, industrData, CancellationToken.None);

        // Assert
        Assert.True(result.Value is OpIndustryFrontEndDto);
        var industriesFrontEnd = result.Value as OpIndustryFrontEndDto;
        Assert.Equal(industrData.Address, industriesFrontEnd!.Address);
        Assert.Equal(industrData.Name, industriesFrontEnd.Name);
        Assert.Equal(industrData.Name, industriesFrontEnd.Name);
        Assert.Equal(industrData.Address, industriesFrontEnd.Address);
        _operatorIndustryRepositoryMock.Verify(r => 
            r.GetByIdAsync(_testOperator.Id, operatorIndustry.Id, CancellationToken.None), Times.Once);
        
        _operatorIndustryRepositoryMock.Verify(r => 
            r.UpdateAsync(operatorIndustry, CancellationToken.None), Times.Once);
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
            r.GetByIdAsync(_testOperator.Id, operatorIndustry.Id, CancellationToken.None)).ReturnsAsync(operatorIndustry);
        
        // Act
        var result = await _operatorIndustryService.RemoveOperatorIndustryAsync(operatorIndustry.Id, CancellationToken.None);
        
        // Assert
        Assert.True(result.Value is int);
        var resultId = (int)result.Value;
        Assert.Equal(operatorIndustry.Id, resultId);
        _operatorIndustryRepositoryMock.Verify(r =>
            r.DeleteAsync(It.Is<OperatorIndustry>(industry => industry.Id == operatorIndustry.Id), CancellationToken.None), Times.Once);
        
        _operatorIndustryRepositoryMock.Verify(r => 
            r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None), Times.Once);
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
            r.GetByIdAsync(_testOperator.Id, CancellationToken.None)).ReturnsAsync(_testOperator);
        
        _operatorIndustryRepositoryMock.Setup(r => 
            r.CreateAsync(It.IsAny<OperatorIndustry>(), CancellationToken.None));
        
        // Act
        var result = await _operatorIndustryService.CreateOperatorIndustryAsync(industryCreationDto, CancellationToken.None);
        
        // Assert
        Assert.True(result.Value is OpIndustryFrontEndDto);
        var industriesFrontEnd = result.Value as OpIndustryFrontEndDto;
        Assert.Equal(industryCreationDto.Name, industriesFrontEnd!.Name);
        Assert.Equal(industryCreationDto.Address, industriesFrontEnd.Address);
        _operatorIndustryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<OperatorIndustry>(), CancellationToken.None), Times.Once);
        _operatorRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), CancellationToken.None), Times.Once);
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
            .Setup(r => r.GetByIdAsync(_testOperator.Id, operatorIndustry.Id, CancellationToken.None))
            .ReturnsAsync(operatorIndustry);
        
        // Act
        var result = await _operatorIndustryService.GetOpIndustryByIdAsync(operatorIndustry.Id, CancellationToken.None);
        
        // Assert
        Assert.True(result.Value is OpIndustryFrontEndDto);
        var industriesFrontEnd = result.Value as OpIndustryFrontEndDto;
        Assert.Equal(operatorIndustry.Address, industriesFrontEnd!.Address);
        Assert.Equal(operatorIndustry.Name, industriesFrontEnd.Name);
        _operatorIndustryRepositoryMock.Verify(r => 
            r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None), Times.Once);
    }

    private readonly Operator _testOperator = new Operator()
    {
        BusinessName = "test", Address = "test",
        Email = "test@test.com", Id = 1, Occupation = "test",
    };
}