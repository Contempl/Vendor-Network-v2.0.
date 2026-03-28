using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IVendorService
{
	Task<OneOf<List<BusinessFrontEndDto>, InvalidOperatorNameError, Error>> SearchOperatorsAsync(OperatorSearchDto operatorSearchDto, CancellationToken cancellationToken);
	Task<OneOf<BusinessFrontEndDto, Error>> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken);
	Task<OneOf<BusinessFrontEndDto, Error>> UpdateVendorAsync(UpdateVendorDto vendorData, CancellationToken cancellationToken);
	Task<OneOf<MailMsg, Error>> InviteVendorUserAsync(EmailForInviteDto email, CancellationToken cancellationToken);
	Task<OneOf<List<VendorFacilityDto>, Error>> GetVendorFacilitiesAsync(CancellationToken cancellationToken);
}
