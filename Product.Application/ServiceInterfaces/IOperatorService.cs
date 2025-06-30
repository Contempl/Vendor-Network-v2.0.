using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Pagination;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorService
{
	Task<Response<List<BusinessFrontEndDto>>> SearchForVendorsAsync(SearchVendorsForIndustriesDto industriesData);
	Task<Response<PagedList<Vendor>>> GetVendorsByNameAsync(VendorSearchDto vendorSearchDto);
	Task<Response<BusinessFrontEndDto>> GetOperatorAsync(int operatorId);
	Task<Response<BusinessFrontEndDto>> RegisterOperatorAsync (int operatorUserId, OperatorRegistrationDto operatorRegistrationData);
	Task<Response<BusinessFrontEndDto>> UpdateOperatorAsync (int operatorId, UpdateOperatorDto operatorUpdateData);
	Task<Response<MailMsg>> InviteOperatorUserAsync (int operatorUserId, EmailForInviteDto operatorUpdateData);
	Task<Response<int>> RemoveOperatorAsync (int operatorId);
}
