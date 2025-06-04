using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IVendorService
{
	Task<Response<BusinessFrontEndDto>> RegisterVendorAsync(int vendorUserId, VendorRegistrationDto registrationData);
	Task<Response<List<BusinessFrontEndDto>>> SearchOperatorsAsync(OperatorSearchDto operatorSearchDto);
	Task<Response<BusinessFrontEndDto>> GetVendorByIdAsync(int vendorId);
	Task<Response<BusinessFrontEndDto>> UpdateVendorAsync(UpdateVendorDto vendorData);
	Task<Response<MailMsg>> InviteVendorUserAsync(EmailForInviteDto email);
	Task<Response<int>> DeleteVendorAsync(int vendorId);
	
}
