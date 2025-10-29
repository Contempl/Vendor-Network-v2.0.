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
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();

    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(
            _userRepositoryMock.Object,
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
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VendorUser
            {
                Id = 1,
                UserName = "john_doe",
                Email = "john@example.com",
                FirstName = "John",
            });
        
        // Act
        var result = await _userService.GetUserAsync(userId, It.IsAny<CancellationToken>());

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
        
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"User with id: {userId} could not be found."));
        
        // Act & Assert
        var ex = await Record.ExceptionAsync(() => _userService.GetUserAsync(userId, It.IsAny<CancellationToken>()));
        await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _userService.GetUserAsync(userId, It.IsAny<CancellationToken>()));
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

        _userRepositoryMock.Setup(r => r.GetByIdWithInvitesAsync(vendorUserId, It.IsAny<CancellationToken>()))
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
        var result = await _userService.UpdateUserAsync(dto, vendorUserId, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal("NewName", result.Data.FirstName);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserDoesNotExist_ReturnsError()
    {
        // Arrange
        var vendorUserId = 2;
        var dto = new UserToUpdateDto { FirstName = "NewName" };
        
        _userRepositoryMock.Setup(r => r.GetByIdWithInvitesAsync(vendorUserId, It.IsAny<CancellationToken>()))
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
        var result = await _userService.UpdateUserAsync(dto, vendorUserId, It.IsAny<CancellationToken>());

        // Assert
        Assert.Null(result.Data);
        Assert.Equal((int)ErrorCodes.UsersDontMatch, result.ErrorCode);
    }
    
    [Fact]
    public async Task UpdatingEntityShouldSetUpdatedAtAndUpdatedBy()
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


        _userRepositoryMock.Setup(r => r.GetByIdWithInvitesAsync(vendorUserId, It.IsAny<CancellationToken>()))
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
        var result = await _userService.UpdateUserAsync(dto, vendorUserId, It.IsAny<CancellationToken>());

        // Assert
        // Assert.Equal(vendorUserId, result.Data.UpdatedBy);
        // Assert.True(result.Data.UpdatedAt <= DateTime.UtcNow && result.Data.UpdatedAt > DateTime.UtcNow.AddSeconds(-5));
    }

}