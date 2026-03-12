using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
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
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, 
        IPasswordHasher passwordHasher, 
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository, 
        IVendorUserRepository vendorUserRepository,
        IOperatorUserRepository operatorUserRepository, 
        IAdministratorRepository administratorRepository,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _adminRepository = administratorRepository;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _vendorUserRepository = vendorUserRepository;
        _operatorUserRepository = operatorUserRepository;
    }

    public async Task<OneOf<TokenDto, NotFoundError, ValidationError, Error>> LoginAdministrator(UserLoginDto userData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var admin = await _adminRepository.GetByEmailAsync(userData.Email, cancellationToken);

            if (admin == null)
            {
                _logger.LogWarning("Admin not found.");
                return new NotFoundError("No admin found");
            }

            var passwordsAreEqual = _passwordHasher.ValidatePassword(userData.Password, admin.PasswordHash!);

            if (!passwordsAreEqual)
            {
                _logger.LogWarning("Invalid password");
                return new ValidationError();
            }

            var userClaims = admin.MapAdminToClaimDto();
            var token = _jwtTokenService.GenerateToken(userClaims);

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while logging as administrator.");
            return new Error();
        }
    }

    public async Task<OneOf<TokenDto, ValidationError, Error>> Refresh(RefreshTokenRequestDto refreshDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshDto.RefreshToken, cancellationToken);

            if (existingToken == null || !_jwtTokenService.Validate(existingToken))
            {
                _logger.LogWarning("Refresh token expired");
                return new ValidationError();
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

            return newToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while refreshing jwt token.");
            return new Error();
        }
    }

    public async Task<OneOf<TokenDto, NotFoundError, ValidationError, Error>> Login(UserLoginDto userData, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(userData.Email, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return new NotFoundError("No user found");
            }

            var passwordsAreEqual = _passwordHasher.ValidatePassword(userData.Password, user.PasswordHash!);

            if (!passwordsAreEqual)
            {
                _logger.LogWarning("Invalid password");
                return new ValidationError();
            }

            var userClaims = user.MapUserToClaimDto();

            var token = _jwtTokenService.GenerateToken(userClaims);

            var refreshToken = new RefreshToken
            {
                Token = token.RefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

            token.RefreshToken = refreshToken.Token;

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while logging.");
            throw;
        }
    }

    public async Task<OneOf<UserDtoToFrontEnd, NotFoundError, Error>> RegisterUser(UserRegistrationDto registrationData,
        CancellationToken cancellationToken = default)
    {
        var userByEmail = await _userRepository.GetByEmailAsync(registrationData.Email, cancellationToken);
        if (userByEmail != null)
        {
            _logger.LogWarning("User with this email already exists.");
            return new NotFoundError();
        }

        if (!registrationData.IsOperator)
        {
            var newVendor = registrationData.MapVendorUserFromDto();
            newVendor.PasswordHash = _passwordHasher.HashThePassword(registrationData.Password);

            await _vendorUserRepository.CreateAsync(newVendor, cancellationToken);
            var vendorUserDto = newVendor.MapToFrontEndDto();
            return vendorUserDto;
        }

        var newOperator = registrationData.MapOperatorUserFromDto();
        newOperator.PasswordHash = _passwordHasher.HashThePassword(registrationData.Password);

        await _operatorUserRepository.CreateAsync(newOperator, cancellationToken);

        var operatorUserDto = newOperator.MapToFrontEndDto();

        return operatorUserDto;
    }
}