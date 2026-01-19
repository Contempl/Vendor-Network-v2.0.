using OneOf;
using OneOf.Types;
using Product.Domain.Dto;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IUserService
{
	Task<OneOf<UserDtoToFrontEnd, Error>> GetUserAsync(int userId, CancellationToken cancellationToken);
	Task<OneOf<int, Error>> RemoveUserAsync(int userId, CancellationToken cancellationToken);
	Task<OneOf<UserDtoToFrontEnd, ValidationError, Error>> UpdateUserAsync(UserToUpdateDto userUpdateData, int userId, CancellationToken cancellationToken);
}
