using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IAdministratorService
{
    Task<Response<UserDtoToFrontEnd>> InviteVendorUser(DataForInviteDto inviteData, CancellationToken cancellationToken);
    Task<Response<UserDtoToFrontEnd>> InviteOperatorUser(DataForInviteDto inviteData, CancellationToken cancellationToken);
    Task<Response<UserDtoToFrontEnd>> InviteBusiness(BusinessInvitationData invitationData, CancellationToken cancellationToken);
    Task<Response<int>> RemoveOperatorAsync(int operatorId, CancellationToken cancellationToken);
    Task<Response<int>> RemoveVendorAsync(int vendorId, CancellationToken cancellationToken);
}
