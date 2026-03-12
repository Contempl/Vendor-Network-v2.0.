using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IAdministratorService
{
    Task<OneOf<UserDtoToFrontEnd, ValidationError, Error>> InviteVendorUser(DataForInviteDto inviteData, CancellationToken cancellationToken);
    Task<OneOf<UserDtoToFrontEnd, ValidationError, Error>> InviteOperatorUser(DataForInviteDto inviteData, CancellationToken cancellationToken);
    Task<OneOf<UserDtoToFrontEnd, ValidationError, Error>> InviteBusiness(BusinessInvitationData invitationData, CancellationToken cancellationToken);
    Task<OneOf<int, Error>> RemoveOperatorAsync(int operatorId, CancellationToken cancellationToken);
    Task<OneOf<int, Error>> RemoveVendorAsync(int vendorId, CancellationToken cancellationToken);
}
