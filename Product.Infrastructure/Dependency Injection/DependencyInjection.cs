using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Infrastructure.Implementations;
using Product.Infrastructure.Repositories;

namespace Product.Infrastructure.Dependency_Injection;

public static class DependencyInjection
{
    public static void AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
       services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
       
       services.InitRepoistories();
    }

    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAdministratorService, AdministratorService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IVendFacilityService, VendFacilityService>();
        services.AddScoped<IVendorUserService, VendorUserService>();
        services.AddScoped<IOperatorService, OperatorService>();
        services.AddScoped<IOperatorIndustryService, OperatorIndustryService>();
        services.AddScoped<IOperatorUserService, OperatorUserService>();
        services.AddScoped<IInviteService, InviteService>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();
    }
    
    private static void InitRepoistories(this IServiceCollection services)
    {
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
    }

    private static void InitServices(this IServiceCollection services)
    {
        
    }
}