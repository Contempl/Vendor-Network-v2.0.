using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IAuthService
{
    Task<Response<TokenDto>> Login(UserLoginDto userData, CancellationToken cancellationToken);
    
    Task<Response<UserDtoToFrontEnd>> RegisterUser(UserRegistrationDto registrationData, CancellationToken cancellationToken);
}