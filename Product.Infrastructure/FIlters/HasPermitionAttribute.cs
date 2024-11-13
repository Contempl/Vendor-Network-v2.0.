using Microsoft.AspNetCore.Authorization;

namespace Product.Infrastructure.FIlters;

public sealed class HasPermitionAttribute : AuthorizeAttribute
{
    public HasPermitionAttribute(Permission permission)
        : base(policy: permission.ToString())
    { }
}
