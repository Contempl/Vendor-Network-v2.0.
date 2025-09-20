using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorIndustryService
{

    Task<Response<OpIndustryFrontEndDto>> UpdateOperatorIndustryAsync(int industryId, UpdateOperatorIndustryDto operatorIndustry, CancellationToken cancellationToken);
    Task<Response<int>> RemoveOperatorIndustryAsync(int industryId, CancellationToken cancellationToken);
    Task<Response<List<OpIndustryFrontEndDto>>> GetOperatorsIndustriesAsync(CancellationToken cancellationToken);
    Task<Response<OpIndustryFrontEndDto>> CreateOperatorIndustryAsync(OperatorIndustryCreationDto industryCreationData, CancellationToken cancellationToken);
    Task<Response<OpIndustryFrontEndDto>> GetOpIndustryByIdAsync(int industryId, CancellationToken cancellationToken);
}
