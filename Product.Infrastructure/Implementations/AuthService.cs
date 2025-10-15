using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;

namespace Product.Infrastructure.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IInviteRepository _inviteRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IVendorUserRepository _vendorUserRepository;
    private readonly IOperatorUserRepository _operatorUserRepository;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService, IRefreshTokenRepository refreshTokenRepository, IVendorUserRepository vendorUserRepository, IOperatorUserRepository operatorUserRepository, IInviteRepository inviteRepository)
    {
        _userRepository = userRepository;
        _inviteRepository = inviteRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _vendorUserRepository = vendorUserRepository;
        _operatorUserRepository = operatorUserRepository;
    }

    public async Task<Response<TokenDto>> Login(UserLoginDto userData, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(userData.Email, cancellationToken);

        if (user == null)
        {
            return new Response<TokenDto>
            {
                ErrorMessage = "User not found",
                ErrorCode = (int)ErrorCodes.UserNotFound
            };
        }

        var passwordsAreEqual = _passwordHasher.ValidatePassword(userData.Password, user.PasswordHash!);

        if (!passwordsAreEqual)
        {
            return new Response<TokenDto>
            {
                ErrorMessage = "Invalid password",
                ErrorCode = (int)ErrorCodes.InvalidPassword
            };
        }

        var userClaims = user.MapUserToClaimDto();
		
        var token = _jwtTokenService.GenerateToken(userClaims);
		
        var refreshToken = new RefreshToken
        {
            Token = _jwtTokenService.GenerateRefreshToken(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
		
        await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);
		
        token.RefreshToken = refreshToken.Token;
		
        return new Response<TokenDto>
        {
            Data = token,
        };
    }

    public async Task<Response<UserDtoToFrontEnd>> RegisterUser(UserRegistrationDto registrationData, 
        CancellationToken cancellationToken)
    {
        var userByEmail = await _userRepository.GetByEmailAsync(registrationData.Email, cancellationToken);
        if (userByEmail != null)
        {
            return new Response<UserDtoToFrontEnd>
            {
                ErrorMessage = "User with this email already exists!",
                ErrorCode = (int)ErrorCodes.UserWithThisEmailAlreadyExists
            };
        }
		
        if (!registrationData.IsOperator)
        {
            var newVendor = MapVendorUserFromDto(registrationData);

            await _vendorUserRepository.CreateAsync(newVendor, cancellationToken);
            var vendorUserDto = newVendor.MapToFrontEndDto();
            return new Response<UserDtoToFrontEnd>
            {
                Data = vendorUserDto
            };
        }
		
        var newOperator = MapOperatorUserFromDto(registrationData);
        await _operatorUserRepository.CreateAsync(newOperator, cancellationToken);
		
        var operatorUserDto = newOperator.MapToFrontEndDto();

        return new Response<UserDtoToFrontEnd>
        {
            Data = operatorUserDto
        };
    }
    
    private VendorUser MapVendorUserFromDto(UserRegistrationDto registrationData) => new VendorUser
    {
        UserName = registrationData.UserName,
        FirstName = registrationData.FirstName,
        LastName = registrationData.LastName,
        Email = registrationData.Email,
        PasswordHash = _passwordHasher.HashThePassword(registrationData.Password),
        UserType = UserType.VendorUser,
    };

    private OperatorUser MapOperatorUserFromDto(UserRegistrationDto registrationData) => new OperatorUser
    {
        UserName = registrationData.UserName,
        FirstName = registrationData.FirstName,
        LastName = registrationData.LastName,
        Email = registrationData.Email,
        PasswordHash = _passwordHasher.HashThePassword(registrationData.Password),
        UserType = UserType.OperatorUser,
    };
}