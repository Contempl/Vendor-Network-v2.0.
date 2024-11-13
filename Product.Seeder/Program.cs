using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Product.Application.Interfaces;
using Product.Domain.Entity;
using Product.Infrastructure;
using Product.Infrastructure.Repositories;

namespace Product.Seeder;

class Program
{
    static void Main(string[] args)
    {

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json");
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
        

        var dbContext = host.Services.GetService<AppDbContext>();

        dbContext.Administrators.Add(new Administrator
        {
            Email = "admin@gmail.com",
            FirstName = "Ashas",
            LastName = "Chivopats",
            Invites = new List<Invite>(),
            PasswordHash = HashThePassword("jopaPopa")
        });

        dbContext.SaveChanges();
    }
    private static byte[] HashThePassword(string password)
    {
        using (var sha = SHA512.Create())
        {
            var passwordHash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return passwordHash;
        }
    }
}