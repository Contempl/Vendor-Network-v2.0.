using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IAdministratorService
{
    Task<Response<UserDtoToFrontEnd>> InviteVendorUser(int adminId, DataForInviteDto inviteData);
    Task<Response<UserDtoToFrontEnd>> InviteOperatorUser(int adminId, DataForInviteDto inviteData);
    Task<Response<UserDtoToFrontEnd>> InviteBusiness(int adminId, BusinessInvitationData invitationData);
}
