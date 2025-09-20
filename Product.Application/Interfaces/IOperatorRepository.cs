using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IOperatorRepository : IRepository<Operator>
{
	Task<Operator> GetByIdAsync(int operatorId, CancellationToken cancellationToken);
	Task<List<Operator>> GetOperatorsByNameAsync(string operatorName, CancellationToken cancellationToken);
}