using Product.Domain.Enum;

namespace Product.WebApi.Configuration;

public static class AuthorizationConfiguration
{
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("SuperAdmin", policy =>
                policy.RequireRole(UserType.SuperAdmin.ToString()));

            options.AddPolicy("VendorUser", policy =>
                policy.RequireAssertion(context => context.User.IsInRole(UserType.VendorUser.ToString()) ||
                    IsBusinessAdmin(context.User, "Vendor")));

            options.AddPolicy("OperatorUser", policy =>
                policy.RequireAssertion(context => context.User.IsInRole(UserType.OperatorUser.ToString()) ||
                    IsBusinessAdmin(context.User, "Operator")));

            options.AddPolicy("VendorAdmin", policy =>
                policy.RequireAssertion(context => IsBusinessAdmin(context.User, "Vendor")));

            options.AddPolicy("OperatorAdmin", policy =>
                policy.RequireAssertion(context => IsBusinessAdmin(context.User, "Operator")));

            options.AddPolicy("All", policy =>
                policy.RequireRole(UserType.VendorUser.ToString(),
                    UserType.OperatorUser.ToString(),
                    UserType.Admin.ToString(), UserType.SuperAdmin.ToString()));
        });

        return services;
    }

    private static bool IsBusinessAdmin(System.Security.Claims.ClaimsPrincipal user, string businessType) =>
        user.IsInRole(UserType.Admin.ToString()) &&
        user.HasClaim("businessType", businessType) &&
        int.TryParse(user.FindFirst("businessId")?.Value, out var businessId) && businessId > 0;
}
