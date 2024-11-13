using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Infrastructure.Implementations;

public class OperatorIndustryService : IOperatorIndustryService
{
    private readonly IOperatorIndustryRepository _operatorIndustryRepository;

    public OperatorIndustryService(IOperatorIndustryRepository operatorIndustryRepository)
    {
        _operatorIndustryRepository = operatorIndustryRepository;
    }

    public Task CreateAsync(OperatorIndustry operatorIndustry) => _operatorIndustryRepository
        .CreateAsync(operatorIndustry);
    public Task DeleteAsync(OperatorIndustry industry) => _operatorIndustryRepository.DeleteAsync(industry);
    public Task<OperatorIndustry> GetByIdAsync(int operatorId, int industryId) => _operatorIndustryRepository
        .GetByIdAsync(operatorId, industryId);
    public async Task<List<OperatorIndustry>> GetOperatorsIndustries(int operatorId)
    {
         var industries = await _operatorIndustryRepository.GetAll()
            .Where(i => i.OperatorId == operatorId).ToListAsync();
        return industries;
    }
    public OperatorIndustry MapIndustryToCreateOperator(Operator @operator, OperatorIndustryCreationDto industryData)
    {
		var newIndustry = new OperatorIndustry
		{
			Name = industryData.Name,
			Address = industryData.Address,
			Latitude = industryData.Latitude,
			Longitude = industryData.Longitude,
			Operator = @operator
		};
        return newIndustry;
	}
    public void MapIndustryToUpdate(OperatorIndustry industry, 
        UpdateOperatorIndustryDto industryData)
    {
		industry.Name = industryData.Name ?? industry.Name;
		industry.Address = industryData.Address ?? industry.Address;
		industry.Latitude = industryData.Latitude ?? industry.Latitude;
		industry.Longitude = industryData.Longitude ?? industry.Longitude;
	}
	public Task UpdateAsync(OperatorIndustry operatorIndustry) => _operatorIndustryRepository
        .UpdateAsync(operatorIndustry);
}
