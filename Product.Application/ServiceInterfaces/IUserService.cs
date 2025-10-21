using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IUserService
{
	Task<Response<UserDtoToFrontEnd>> GetUserAsync(int userId, CancellationToken cancellationToken);
	Task<Response<int>> RemoveUserAsync(int userId, CancellationToken cancellationToken);
	Task<Response<UserDtoToFrontEnd>> UpdateUserAsync(UserToUpdateDto userUpdateData, int userId, CancellationToken cancellationToken);
	Task<Response<TokenDto>> Refresh(RefreshTokenRequestDto refreshDto, CancellationToken cancellationToken);
}
