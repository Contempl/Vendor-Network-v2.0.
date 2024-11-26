using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.Interfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Exceptions;

namespace Product.Infrastructure.Filters;

public class EnsureEntityExists<T, TRepository> : ActionFilterAttribute 
	where TRepository : IRepository<T> 
	where T : IEntity
{
	public virtual string ExpectedParameterId => throw new NotImplementedException();
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var service = context.HttpContext.RequestServices.GetService<TRepository>()!;
		var entity = await service.GetByIdOrDefaultAsync((int)context.ActionArguments[ExpectedParameterId]);
		
		if (entity == null)
		{
			throw new NotFoundException<T>();
		}

		await next();
	}
}

public class EnsureAdministratorExists : EnsureEntityExists<Administrator, IAdministratorRepository>
{
	public override string ExpectedParameterId => "adminId";
}
public class EnsureInviteExists : EnsureEntityExists<Invite, IInviteRepository>
{
	public override string ExpectedParameterId => "inviteId";
}
public class EnsureUserExists : EnsureEntityExists<User, IUserRepository>
{
	public override string ExpectedParameterId => "userId";
}
public class EnsureOperatorExists : EnsureEntityExists<Operator, IOperatorRepository>
{
	public override string ExpectedParameterId => "operatorId";
}
public class EnsureOperatorUserExists : EnsureEntityExists<OperatorUser, IOperatorUserRepository>
{
	public override string ExpectedParameterId => "operatorUserId";
}
public class EnsureOperatorIndustryExists : EnsureEntityExists<OperatorIndustry, IOperatorIndustryRepository>
{
	public override string ExpectedParameterId => "industryId";
}
public class EnsureVendorExists : EnsureEntityExists<Vendor, IVendorRepository>
{
	public override string ExpectedParameterId => "vendorId";
}
public class EnsureVendorUserExists : EnsureEntityExists<VendorUser, IVendorUserRepository>
{
	public override string ExpectedParameterId => "vendorUserId";
}
public class EnsureVendorFacilityExists : EnsureEntityExists<VendorFacility, IVendorFacilityRepository>
{
	public override string ExpectedParameterId => "facilityId";
}
public class EnsureVendorFacilityServiceExists : EnsureEntityExists<VendorFacilityService, IVendorFacilityServiceRepository>
{
	public override string ExpectedParameterId => "facilityServiceId";
}