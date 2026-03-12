using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IAuthService
{
    Task<OneOf<TokenDto, NotFoundError, ValidationError, Error>> Login(UserLoginDto userData, CancellationToken cancellationToken);
    Task<OneOf<UserDtoToFrontEnd, NotFoundError, Error>> RegisterUser(UserRegistrationDto registrationData, CancellationToken cancellationToken);
    Task<OneOf<TokenDto, NotFoundError, ValidationError, Error>> LoginAdministrator(UserLoginDto userData, CancellationToken cancellationToken);
    Task<OneOf<TokenDto, ValidationError, Error>> Refresh(RefreshTokenRequestDto refreshDto, CancellationToken cancellationToken);
}