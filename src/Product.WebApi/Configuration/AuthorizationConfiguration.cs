using Product.Domain.Enum;

namespace Product.WebApi.Configuration;

public static class AuthorizationConfiguration
{
    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
                policy.RequireRole(UserType.Admin.ToString()));

            options.AddPolicy("VendorUser", policy =>
                policy.RequireRole(UserType.VendorUser.ToString(), UserType.Admin.ToString()));

            options.AddPolicy("OperatorUser", policy =>
                policy.RequireRole(UserType.OperatorUser.ToString(), UserType.Admin.ToString()));

            options.AddPolicy("All", policy =>
                policy.RequireRole(UserType.VendorUser.ToString(),
                    UserType.OperatorUser.ToString(),
                    UserType.Admin.ToString()));
        });

        return services;
    }
}