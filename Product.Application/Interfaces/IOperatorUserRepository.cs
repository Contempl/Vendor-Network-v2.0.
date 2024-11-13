using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IOperatorUserRepository : IRepository<OperatorUser>
{
	Task<OperatorUser> GetByIdAsync(int operatorId);
}