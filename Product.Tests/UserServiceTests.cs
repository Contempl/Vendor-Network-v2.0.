using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IVendorUserRepository> _vendorUserRepositoryMock = new();
    private readonly Mock<IOperatorUserRepository> _operatorUserRepositoryMock = new();
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock = new();
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new();

    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenServiceMock.Object,
            _vendorUserRepositoryMock.Object,
            _operatorUserRepositoryMock.Object,
            _redisCacheServiceMock.Object,
            _userPrincipalServiceMock.Object
        );
    }

    [Fact]
    public async Task GetUserAsync_WhenUserExists_ReturnsUserDto()
    {
        // Arrange
        var userId = 1;
        var user = new VendorUser
        {
            Id = userId,
            UserName = "john_doe",
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new VendorUser
            {
                Id = 1,
                UserName = "john_doe",
                Email = "john@example.com",
                FirstName = "John",
            });
        
        // Act
        var result = await _userService.GetUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("John", result.Data.FirstName);
        Assert.Equal("john@example.com", result.Data.Email);
    }
    
    [Fact]
    public async Task GetUserAsync_WhenUserDoesNotExist_ThrowsException()
    {
        // Arrange
        var userId = 999;
        
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ThrowsAsync(new KeyNotFoundException($"User with id: {userId} could not be found."));
        
        // Act & Assert
        var ex = await Record.ExceptionAsync(() => _userService.GetUserAsync(userId));
        await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _userService.GetUserAsync(userId));
    }

    
    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_Updates()
    {
        // Arrange
        var vendorUserId = 1;
        var dto = new UserToUpdateDto { FirstName = "NewName" };
        
        var users = new List<User>
        {
            new VendorUser
            {
                Id = vendorUserId,
                FirstName = "OldName",
                SentInvites = new List<Invite>()
            }
        }.AsQueryable();

        var dbSetMock = new Mock<DbSet<User>>();
        dbSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        dbSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        dbSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        dbSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _userRepositoryMock.Setup(r => r.GetByIdWithInvitesAsync(vendorUserId))
            .ReturnsAsync(new VendorUser
            {
                Id = vendorUserId,
                FirstName = "OldName",
                SentInvites = new List<Invite> { new Invite { Id = 1 } }
            });
        
        _userPrincipalServiceMock
            .Setup(x => x.UserId)
            .Returns(vendorUserId);
        
        // Act
        var result = await _userService.UpdateUserAsync(dto, vendorUserId);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal("NewName", result.Data.FirstName);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserDoesNotExist_ReturnsError()
    {
        // Arrange
        var vendorUserId = 2;
        var dto = new UserToUpdateDto { FirstName = "NewName" };
        
        _userRepositoryMock.Setup(r => r.GetByIdWithInvitesAsync(vendorUserId))
            .ReturnsAsync(new VendorUser
            {
                Id = vendorUserId,
                FirstName = "OldName",
                SentInvites = new List<Invite> { new Invite { Id = 1 } }
            });

        _userPrincipalServiceMock
            .Setup(x => x.UserId)
            .Returns(3);
        

        // Act
        var result = await _userService.UpdateUserAsync(dto, vendorUserId);

        // Assert
        Assert.Null(result.Data);
        Assert.Equal((int)ErrorCodes.UsersDontMatch, result.ErrorCode);
    }

    
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        //Arrange
        var email = "test@example.com";
        var password = "password";
        var user = new VendorUser { Id = 1, Email = email, PasswordHash = Encoding.UTF8.GetBytes("hashed") };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.ValidatePassword(password, user.PasswordHash)).Returns(true);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns
        (
            new TokenDto
            {
                AccessToken = "fake_token",
            }
        );
        
        //Act
        var result = await _userService.Login
            (
                new UserLoginDto
                {
                    Email = email,
                    Password = password
                }
            );

        //Assert
        Assert.NotNull(result.Data);
        Assert.Equal("fake_token", result.Data.AccessToken);
    }
    
    [Fact]
    public async Task LoginAsync_InvalidEmail_ReturnsError()
    {
        //Arrange
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null!);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(new TokenDto { AccessToken = "log_token" });
        
        //Act
        var result = await _userService.Login(new UserLoginDto { Email = "notfound@example.com", Password = "123" });

        //Assert
        Assert.Null(result.Data);
        Assert.Equal((int)ErrorCodes.UserNotFound, result.ErrorCode);
        _jwtTokenServiceMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsError()
    {
        //Arrange
        var user = new VendorUser { Id = 1, Email = "test@example.com", PasswordHash = Encoding.UTF8.GetBytes("hash") };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.ValidatePassword("wrongpass", user.PasswordHash)).Returns(false);

        //Act
        var result = await _userService.Login(new UserLoginDto { Email = user.Email, Password = "wrongpass" });

        //Assert
        Assert.Null(result.Data);
        Assert.Equal((int)ErrorCodes.InvalidPassword, result.ErrorCode);
        _passwordHasherMock.Verify(h => h.ValidatePassword("wrongpass", user.PasswordHash), Times.Once);
    }

    
    [Fact]
    public async Task RegisterAsync_ReturnsData_WithValidVendorUserData()
    {
        //Arrange
        var dto = new UserRegistrationDto { Email = "new@example.com", Password = "pass123" };
    
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        //Act
        var result = await _userService.RegisterUser(dto);
    
        //Assert
        Assert.NotNull(result.Data);
        Assert.Equal("new@example.com", result.Data.Email);
        _vendorUserRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<VendorUser>()), Times.Once);
    }
    
    [Fact]
    public async Task RegisterAsync_ReturnsData_WithValidOperatorUserData()
    {
        //Arrange
        var dto = new UserRegistrationDto { Email = "new@example.com", Password = "pass123", IsOperator = true};
    
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        //Act
        var result = await _userService.RegisterUser(dto);
    
        //Assert
        Assert.NotNull(result.Data);
        Assert.Equal("new@example.com", result.Data.Email);
        _operatorUserRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<OperatorUser>()), Times.Once);
    }
    
    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ReturnsError()
    {
        //Arrange
        var dto = new UserRegistrationDto { Email = "existing@example.com" };
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new VendorUser
        {
            Email = "existing@example.com",
        });

        //Act
        var result = await _userService.RegisterUser(dto);

        //Assert
        Assert.Null(result.Data);
        Assert.Equal((int)ErrorCodes.UserWithThisEmailAlreadyExists, result.ErrorCode);
    }

}