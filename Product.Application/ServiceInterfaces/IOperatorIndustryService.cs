using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorIndustryService
{

    Task<Response<OpIndustryFrontEndDto>> UpdateOperatorIndustryAsync(int industryId, UpdateOperatorIndustryDto operatorIndustry);
    Task<Response<int>> RemoveOperatorIndustryAsync(int industryId);
    Task<Response<List<OpIndustryFrontEndDto>>> GetOperatorsIndustriesAsync();
    Task<Response<OpIndustryFrontEndDto>> CreateOperatorIndustryAsync(OperatorIndustryCreationDto industryCreationData);
    Task<Response<OpIndustryFrontEndDto>> GetOpIndustryByIdAsync(int industryId);
}
