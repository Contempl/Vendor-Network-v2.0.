using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IAdministratorService
{
    Task<Response<TokenDto>> Login(UserLoginDto userData, CancellationToken cancellationToken);
    Task<Response<UserDtoToFrontEnd>> InviteVendorUser(int adminId, DataForInviteDto inviteData, CancellationToken cancellationToken);
    Task<Response<UserDtoToFrontEnd>> InviteOperatorUser(int adminId, DataForInviteDto inviteData, CancellationToken cancellationToken);
    Task<Response<UserDtoToFrontEnd>> InviteBusiness(int adminId, BusinessInvitationData invitationData, CancellationToken cancellationToken);
    Task<Response<int>> RemoveOperatorAsync(int operatorId, CancellationToken cancellationToken);
    Task<Response<int>> RemoveVendorAsync(int vendorId, CancellationToken cancellationToken);
}
