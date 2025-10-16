using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Settings;
using Product.Infrastructure.Implementations;
using Product.Infrastructure.Implementations.Account;
using Product.Infrastructure.Interceptors;
using Product.Infrastructure.Repositories;

namespace Product.Infrastructure.Dependency_Injection;

public static class DependencyInjection
{
    public static void AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    { 
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddSingleton<DateInterceptor>();
        
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
       
       services.InitRepoistories();
    }

    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(nameof(RedisSettings));
        var redisSettings = new RedisSettings(options["Url"], options["InstanceName"]);
        
        services.AddStackExchangeRedisCache(redisOptions =>
        {
            redisOptions.Configuration = redisSettings.Url;
            redisOptions.InstanceName = redisSettings.InstanceName;
        });
        services.InitServices(configuration);
    }
    
    private static void InitRepoistories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAdministratorRepository, AdministratorRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInviteRepository, InviteRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IVendorFacilityRepository, VendorFacilityRepository>();
        services.AddScoped<IVendorFacilityServiceRepository, VendorFacilityServiceRepository>();
        services.AddScoped<IVendorUserRepository, VendorUserRepository>();
        services.AddScoped<IOperatorRepository, OperatorRepository>();
        services.AddScoped<IOperatorIndustryRepository, OperatorIndustryRepository>();
        services.AddScoped<IOperatorUserRepository, OperatorUserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }

    private static void InitServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAdministratorService, AdministratorService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IVendFacilityService, VendFacilityService>();
        services.AddScoped<IOperatorService, OperatorService>();
        services.AddScoped<IOperatorIndustryService, OperatorIndustryService>();
        services.AddScoped<IInviteService, InviteService>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<IEmailService, EmailService>(provider =>
        {
            var urlHelperFactory = provider.GetRequiredService<IUrlHelperFactory>();
            var actionContextAccessor = provider.GetRequiredService<IActionContextAccessor>();
            var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            return new EmailService(configuration ,urlHelper);
        });
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserPrincipalService, UserPrincipalService>();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddHttpContextAccessor();
        services.AddScoped<ClaimsPrincipal>(serviceProvider => serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.User);
    }
}