using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorIndustryService
{

    Task<OneOf<OpIndustryFrontEndDto, NotFoundError, Error>> UpdateOperatorIndustryAsync(int industryId, UpdateOperatorIndustryDto operatorIndustry, CancellationToken cancellationToken);
    Task<OneOf<int, Error>> RemoveOperatorIndustryAsync(int industryId, CancellationToken cancellationToken);
    Task<OneOf<List<OpIndustryFrontEndDto>, Error>> GetOperatorsIndustriesAsync(CancellationToken cancellationToken);
    Task<OneOf<OpIndustryFrontEndDto, Error>> CreateOperatorIndustryAsync(OperatorIndustryCreationDto industryCreationData, CancellationToken cancellationToken);
    Task<OneOf<OpIndustryFrontEndDto, Error>> GetOpIndustryByIdAsync(int industryId, CancellationToken cancellationToken);
}
