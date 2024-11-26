using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.ServiceInterfaces;
using Product.Infrastructure.Exceptions;

namespace Product.Infrastructure.Filters;


public class EnsureBusinessAccess : ActionFilterAttribute 
{
    public string UserType { get; }
    
    public EnsureBusinessAccess(string userType)
    {
        UserType = userType;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userPrincipalService =  context.HttpContext.RequestServices.GetRequiredService<IUserPrincipalService>();
        if (UserType != userPrincipalService.UserType)
        {
            throw new NotFoundException($"User with type {userPrincipalService.UserType} tried to access {UserType} resource. \n UserId: {userPrincipalService.UserId}. \n UserType: {UserType}");
        }
    }
}