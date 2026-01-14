using System.Text;
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

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new ();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IVendorUserRepository> _vendorUserRepositoryMock = new();
    private readonly Mock<IOperatorUserRepository> _operatorUserRepositoryMock = new();
    private readonly Mock<IAdministratorRepository> _adminRepositoryMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _vendorUserRepositoryMock.Object,
            _operatorUserRepositoryMock.Object,
            _adminRepositoryMock.Object,
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        //Arrange
        var email = "test@example.com";
        var password = "password";
        var user = new VendorUser { Id = 1, Email = email, PasswordHash = Encoding.UTF8.GetBytes("hashed"), VendorId = 1};

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.ValidatePassword(password, user.PasswordHash)).Returns(true);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(It.IsAny<UserClaimDto>())).Returns
        (
            new TokenDto
            {
                AccessToken = "fake_token",
            }
        );
        
        //Act
        var result = await _authService.Login
        (
            new UserLoginDto
            {
                Email = email,
                Password = password
            }, 
            It.IsAny<CancellationToken>()
        );

        //Assert
        Assert.True(result.Value is TokenDto);
        var jwtToken = result.Value as TokenDto;
        Assert.Equal("fake_token", jwtToken.AccessToken);
    }
    [Fact]
    public async Task LoginAsync_InvalidEmail_ReturnsError()
    {
        //Arrange
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(It.IsAny<UserClaimDto>())).Returns(new TokenDto { AccessToken = "log_token" });
        
        //Act
        var result = await _authService.Login(new UserLoginDto { Email = "notfound@example.com", Password = "123" }, It.IsAny<CancellationToken>());

        //Assert
        Assert.True(result.Value is NotFoundError);
        _jwtTokenServiceMock.Verify(j => j.GenerateToken(It.IsAny<UserClaimDto>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsError()
    {
        //Arrange
        var user = new VendorUser { Id = 1, Email = "test@example.com", PasswordHash = Encoding.UTF8.GetBytes("hash") };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.ValidatePassword("wrongpass", user.PasswordHash))
            .Returns(false);

        //Act
        var result = await _authService.Login(new UserLoginDto { Email = user.Email, Password = "wrongpass" }, It.IsAny<CancellationToken>());

        //Assert
        Assert.True(result.Value is ValidationError);
        _passwordHasherMock.Verify(h => h.ValidatePassword("wrongpass", user.PasswordHash), Times.Once);
    }

    
    [Fact]
    public async Task RegisterAsync_ReturnsData_WithValidVendorUserData()
    {
        //Arrange
        var dto = new UserRegistrationDto { Email = "new@example.com", Password = "pass123" };
    
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        //Act
        var result = await _authService.RegisterUser(dto, It.IsAny<CancellationToken>());
    
        //Assert
        Assert.True(result.Value is UserDtoToFrontEnd);
        var resultDto = result.Value as UserDtoToFrontEnd;
        Assert.Equal("new@example.com", resultDto!.Email);
        _vendorUserRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<VendorUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task RegisterAsync_ReturnsData_WithValidOperatorUserData()
    {
        //Arrange
        var dto = new UserRegistrationDto { Email = "new@example.com", Password = "pass123", IsOperator = true};
    
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        //Act
        var result = await _authService.RegisterUser(dto, It.IsAny<CancellationToken>());
    
        //Assert
        Assert.True(result.Value is UserDtoToFrontEnd);
        var resultDto = result.Value as UserDtoToFrontEnd;
        Assert.Equal("new@example.com", resultDto!.Email);
        _operatorUserRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<OperatorUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAdminAsync_ReturnsToken_WithValidAdministratorUserData()
    {
        // Arrange
        var adminLoginData = new UserLoginDto { Email = "admin@example.com", Password = "pass123" };
        var admin = new Administrator {Id = 1, Email = "admin@example.com", PasswordHash = "hashed"u8.ToArray()};
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(adminLoginData.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);
        _passwordHasherMock.Setup(h => h.ValidatePassword(adminLoginData.Password, admin.PasswordHash))
            .Returns(true);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(It.IsAny<UserClaimDto>()))
            .Returns(new TokenDto { AccessToken = "fake_token" });
        
        // Act
        var result = await _authService.LoginAdministrator(adminLoginData, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is TokenDto);
    }
    
    [Fact]
    public async Task LoginAdminAsync_ReturnsError_WithInvalidAdministratorEmail()
    {
        // Arrange
        var adminLoginData = new UserLoginDto { Email = "admin@example.com", Password = "pass123" };
        
        // Act
        var result = await _authService.LoginAdministrator(adminLoginData, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is NotFoundError);
    }
    
    [Fact]
    public async Task LoginAdminAsync_ReturnsError_WithInvalidAdministratorPassword()
    {
        // Arrange
        var adminLoginData = new UserLoginDto { Email = "admin@example.com", Password = "pass123" };
        var admin = new Administrator {Id = 1, Email = "admin@example.com", PasswordHash = "hashed"u8.ToArray()};
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(adminLoginData.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);
        
        // Act
        var result = await _authService.LoginAdministrator(adminLoginData, It.IsAny<CancellationToken>());

        // Assert
        Assert.True(result.Value is ValidationError);
    }
}