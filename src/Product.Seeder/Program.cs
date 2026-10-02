using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Product.Application.Interfaces;
using Product.Domain.Entity;
using Product.Infrastructure;
using Product.Infrastructure.Repositories;
using Product.Infrastructure.Implementations.Account;

namespace Product.Seeder;

class Program
{
    static void Main(string[] args)
    {

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                });
            })
            .Build(); 
        

        var email = host.Services.GetRequiredService<IConfiguration>()["SeedAdmin:Email"];
        var password = host.Services.GetRequiredService<IConfiguration>()["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Set SeedAdmin__Email and SeedAdmin__Password before seeding a SuperAdmin.");

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (dbContext.Administrators.Any(admin => admin.UserType == Product.Domain.Enum.UserType.SuperAdmin))
            return;

        dbContext.Administrators.Add(new Administrator
        {
            Email = email.Trim(),
            SentInvites = new List<Invite>(),
            UserType = Product.Domain.Enum.UserType.SuperAdmin,
            PasswordHash = new PasswordHasher().HashThePassword(password)
        });

        dbContext.SaveChanges();
    }
}
