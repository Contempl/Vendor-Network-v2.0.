using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.ServiceInterfaces;
using Product.Domain.Enum;
using Product.Infrastructure.Exceptions;

namespace Product.Infrastructure.Filters;


public class EnsureBusinessAccess : ActionFilterAttribute 
{
    private UserType UserType { get; }
    
    public EnsureBusinessAccess(UserType userType)
    {
        UserType = userType;
    }
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userPrincipalService =  context.HttpContext.RequestServices.GetRequiredService<IUserPrincipalService>();
        var expectedBusinessType = UserType == UserType.VendorUser ? "Vendor" : "Operator";
        var isLocalAdmin = userPrincipalService.UserType == UserType.Admin &&
            userPrincipalService.BusinessType == expectedBusinessType && userPrincipalService.BusinessId > 0;
        if (UserType != userPrincipalService.UserType && !isLocalAdmin)
        {
            throw new NotFoundException($"User with type {userPrincipalService.UserType} tried to access {UserType} resource. \n UserId: {userPrincipalService.UserId}. \n UserType: {UserType}");
        }
    }
}
