using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IVendorService
{
	Task<Response<List<BusinessFrontEndDto>>> SearchOperatorsAsync(OperatorSearchDto operatorSearchDto, CancellationToken cancellationToken);
	Task<Response<BusinessFrontEndDto>> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken);
	Task<Response<BusinessFrontEndDto>> UpdateVendorAsync(UpdateVendorDto vendorData, CancellationToken cancellationToken);
	Task<Response<MailMsg>> InviteVendorUserAsync(EmailForInviteDto email, CancellationToken cancellationToken);
}
