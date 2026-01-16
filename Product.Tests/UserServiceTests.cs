using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Types;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;
using Product.Infrastructure.Implementations;
using Xunit;

namespace Product.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUserPrincipalService> _userPrincipalServiceMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();

    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(
            _userRepositoryMock.Object,
            _userPrincipalServiceMock.Object,
            _loggerMock.Object
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
            .Setup(x => x.GetByIdAsync(userId, CancellationToken.None))
            .ReturnsAsync(new VendorUser
            {
                Id = 1,
                UserName = "john_doe",
                Email = "john@example.com",
                FirstName = "John",
            });
        
        // Act
        var result = await _userService.GetUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.Value is UserDtoToFrontEnd);
        var resultDto = result.Value as UserDtoToFrontEnd;
        Assert.Equal("John", resultDto!.FirstName);
        Assert.Equal("john@example.com", resultDto.Email);
    }
    
    [Fact]
    public async Task GetUserAsync_RepositoryThrows_ReturnsError()
    {
        // Arrange
        var userId = 999;
    
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"User with id: {userId} could not be found."));
    
        // Act & Assert
        var ex = await Record.ExceptionAsync(
            () => _userService.GetUserAsync(userId, CancellationToken.None));
    
        Assert.NotNull(ex);
        Assert.IsType<KeyNotFoundException>(ex); 
    }
    
    [Fact]
    public async Task GetUserAsync_ThrowsKeyNotFound()
    {
        // Arrange
        var userId = 999;
    
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));
    
        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(
            () => _userService.GetUserAsync(userId, CancellationToken.None));
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

        _userRepositoryMock.Setup(r => r.GetByIdWithInvitesAsync(vendorUserId, CancellationToken.None))
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
        var result = await _userService.UpdateUserAsync(dto, vendorUserId, CancellationToken.None);

        // Assert
        Assert.True(result.Value is UserDtoToFrontEnd);
        var resultDto = result.Value as UserDtoToFrontEnd;
        Assert.Equal("NewName", resultDto!.FirstName);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserDoesNotExist_ReturnsError()
    {
        // Arrange
        var vendorUserId = 2;
        var dto = new UserToUpdateDto { FirstName = "NewName" };
        
        _userRepositoryMock.Setup(r => r.GetByIdWithInvitesAsync(vendorUserId, CancellationToken.None))
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
        var result = await _userService.UpdateUserAsync(dto, vendorUserId, CancellationToken.None);

        // Assert
        Assert.True(result.Value is ValidationError);
    }
}