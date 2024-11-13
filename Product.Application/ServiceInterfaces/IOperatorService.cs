using Microsoft.Data.SqlClient;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Application.ServiceInterfaces;

public interface IOperatorService
{
    Task CreateAsync(Operator @operator);
    Task<Operator?> GetByIdOrDefaultAsync(int operatorId);
    Task<Operator> GetByIdAsync(int operatorId);
    Task UpdateAsync(Operator @operator);
    Task DeleteAsync(Operator @operator);
    List<OperatorIndustry> GetAllOperatorIndustries(List<int> facilityIds);
	Task<PagedResult<Vendor>> GetVendorsQuery(string searchName, SortOrder sortOrder,
		int pageSize, int pageNumber);
    void ValidateStringInput(string vendorName);
    Operator MapOperatorFromDto(OperatorRegistrationDto operatorRegistrationData, OperatorUser user);
    void MapOperatorFromDto(Operator @operator, UpdateOperatorDto operatorData);
	Task<List<Vendor>> SearchVendorsAsync(string serviceType, List<OperatorIndustry> operatorFacilities);
}
