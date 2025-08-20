using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IAdministratorService
{
    Task<Response<TokenDto>> Login(UserLoginDto userData);
    Task<Response<UserDtoToFrontEnd>> InviteVendorUser(int adminId, DataForInviteDto inviteData);
    Task<Response<UserDtoToFrontEnd>> InviteOperatorUser(int adminId, DataForInviteDto inviteData);
    Task<Response<UserDtoToFrontEnd>> InviteBusiness(int adminId, BusinessInvitationData invitationData);
    Task<Response<int>> RemoveOperatorAsync(int operatorId);
    Task<Response<int>> RemoveVendorAsync(int vendorId);
}
