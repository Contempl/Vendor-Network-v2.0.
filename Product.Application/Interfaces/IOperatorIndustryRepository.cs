using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IOperatorIndustryRepository : IRepository<OperatorIndustry>
{
	Task<OperatorIndustry> GetByIdAsync(int operatorId, int industryId);
}