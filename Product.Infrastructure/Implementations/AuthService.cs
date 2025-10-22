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
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IVendorUserRepository _vendorUserRepository;
    private readonly IOperatorUserRepository _operatorUserRepository;
    private readonly IAdministratorRepository _adminRepository;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository, IVendorUserRepository vendorUserRepository,
        IOperatorUserRepository operatorUserRepository, IAdministratorRepository administratorRepository)
    {
        _userRepository = userRepository;
        _adminRepository = administratorRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _vendorUserRepository = vendorUserRepository;
        _operatorUserRepository = operatorUserRepository;
    }

    public async Task<Response<TokenDto>> LoginAdministrator(UserLoginDto userData,
        CancellationToken cancellationToken = default)
    {
        var admin = await _adminRepository.GetByEmailAsync(userData.Email, cancellationToken);

        if (admin == null)
        {
            return new Response<TokenDto>
            {
                ErrorMessage = "Admin not found",
                ErrorCode = (int)ErrorCodes.UserNotFound
            };
        }

        var passwordsAreEqual = _passwordHasher.ValidatePassword(userData.Password, admin.PasswordHash!);

        if (!passwordsAreEqual)
        {
            return new Response<TokenDto>
            {
                ErrorMessage = "Invalid password",
                ErrorCode = (int)ErrorCodes.InvalidPassword
            };
        }

        var userClaims = admin.MapAdminToClaimDto();
        var token = _jwtTokenService.GenerateToken(userClaims);

        return new Response<TokenDto>
        {
            Data = token,
        };
    }

    public async Task<Response<TokenDto>> Refresh(RefreshTokenRequestDto refreshDto,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshDto.RefreshToken, cancellationToken);

        if (existingToken == null || !_jwtTokenService.Validate(existingToken))
            return new Response<TokenDto>
            {
                ErrorMessage = "Refresh token expired",
                ErrorCode = (int)ErrorCodes.InvalidRefreshToken
            };

        await _refreshTokenRepository.RevokeAsync(existingToken, cancellationToken);

        var userId = existingToken.UserId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        var userClaims = user.MapUserToClaimDto();

        var newToken = _jwtTokenService.GenerateToken(userClaims);

        var refreshToken = new RefreshToken
        {
            Token = newToken.RefreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

        return new Response<TokenDto>
        {
            Data = newToken
        };
    }

    public async Task<Response<TokenDto>> Login(UserLoginDto userData, CancellationToken cancellationToken = default)
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
        CancellationToken cancellationToken = default)
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
            var newVendor = registrationData.MapVendorUserFromDto();
            newVendor.PasswordHash = _passwordHasher.HashThePassword(registrationData.Password);

            await _vendorUserRepository.CreateAsync(newVendor, cancellationToken);
            var vendorUserDto = newVendor.MapToFrontEndDto();
            return new Response<UserDtoToFrontEnd>
            {
                Data = vendorUserDto
            };
        }

        var newOperator = registrationData.MapOperatorUserFromDto();
        newOperator.PasswordHash = _passwordHasher.HashThePassword(registrationData.Password);

        await _operatorUserRepository.CreateAsync(newOperator, cancellationToken);

        var operatorUserDto = newOperator.MapToFrontEndDto();

        return new Response<UserDtoToFrontEnd>
        {
            Data = operatorUserDto
        };
    }
}