using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Product.Application.ServiceInterfaces;
using Product.Domain.Enum;
using Product.Infrastructure;
using Product.Infrastructure.Interceptors;
using Testcontainers.PostgreSql;

namespace Product.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Product.WebApi.Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer =
        new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            builder.UseEnvironment("Development"); 
            
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole(); 
                logging.SetMinimumLevel(LogLevel.Error); 
            });
            
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDistributedCache>();
            services.RemoveAll<DateInterceptor>();
            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.RemoveAll<IAuthenticationHandlerProvider>();
            services.RemoveAll<IAuthenticationService>();

            services.AddDistributedMemoryCache();
            
            services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(_dbContainer.GetConnectionString()); });
            
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultForbidScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            
            services.AddScoped<IUserPrincipalService, FakeUserPrincipalService>();
        });
    }
    
    public async Task SeedAsync(Func<AppDbContext, Task> seeder)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await seeder(context);
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.ExecuteSqlRawAsync("""
                                                      TRUNCATE TABLE "Businesses", "User", "VendorUsers"
                                                      RESTART IDENTITY CASCADE;
                                                  """);
    }
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        var client = CreateClient();
        
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}

public class FakeUserPrincipalService : IUserPrincipalService
{
    public int? UserId { get; set; } = 1;
    public UserType? UserType { get; set; } = Domain.Enum.UserType.VendorUser;
    public int? BusinessId { get; set; } = 10;
}