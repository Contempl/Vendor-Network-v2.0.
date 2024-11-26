using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorIndustryService
{
    Task CreateAsync(OperatorIndustry operatorIndustry);
    Task<OperatorIndustry> GetByIdAsync(int operatorId, int industryId);
    Task UpdateAsync(OperatorIndustry operatorIndustry);
    Task DeleteAsync(OperatorIndustry industry);
    Task<List<OperatorIndustry>> GetOperatorsIndustries(int operatorId);
    OperatorIndustry MapIndustryToCreateOperator(Operator @operator, OperatorIndustryCreationDto industryData);
    void MapIndustryToUpdate(OperatorIndustry industry, UpdateOperatorIndustryDto industryData);
}
