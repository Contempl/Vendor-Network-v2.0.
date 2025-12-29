using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IOperatorUserRepository : IRepository<OperatorUser>
{
	Task<OperatorUser> GetByIdAsync(int operatorId, CancellationToken cancellationToken);
	
	Task<OperatorUser> CreateAsync(OperatorUserCreationDto userCreationDto, CancellationToken cancellationToken);
}