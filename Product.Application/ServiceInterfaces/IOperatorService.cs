using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Pagination;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorService
{
	Task<Response<List<BusinessFrontEndDto>>> SearchForVendorsAsync(SearchVendorsForIndustriesDto industriesData, CancellationToken cancellationToken);
	Task<Response<PagedList<Vendor>>> GetVendorsByNameAsync(VendorSearchDto vendorSearchDto, CancellationToken cancellationToken);
	Task<Response<BusinessFrontEndDto>> GetOperatorAsync(int operatorId, CancellationToken cancellationToken);
	Task<Response<BusinessFrontEndDto>> UpdateOperatorAsync (int operatorId, UpdateOperatorDto operatorUpdateData, CancellationToken cancellationToken);
	Task<Response<MailMsg>> InviteOperatorUserAsync (int operatorUserId, EmailForInviteDto operatorUpdateData, CancellationToken cancellationToken);
}
