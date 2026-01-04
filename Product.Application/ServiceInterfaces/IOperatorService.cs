using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Pagination;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorService
{
	Task<OneOf<List<BusinessFrontEndDto>, ValidationError, Error>> SearchForVendorsAsync(SearchVendorsForIndustriesDto industriesData, CancellationToken cancellationToken);
	Task<OneOf<PagedList<Vendor>, Error>> GetVendorsByNameAsync(VendorSearchDto vendorSearchDto, CancellationToken cancellationToken);
	Task<OneOf<BusinessFrontEndDto, Error>> GetOperatorAsync(int operatorId, CancellationToken cancellationToken);
	Task<OneOf<BusinessFrontEndDto, Error>> UpdateOperatorAsync (UpdateOperatorDto operatorUpdateData, CancellationToken cancellationToken);
	Task<OneOf<MailMsg, TransactionError>> InviteOperatorUserAsync (EmailForInviteDto operatorUpdateData, CancellationToken cancellationToken);
}
