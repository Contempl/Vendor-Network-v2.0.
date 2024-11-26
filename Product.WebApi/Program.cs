using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Infrastructure;
using Product.Infrastructure.Extensions;
using Product.Infrastructure.Implementations;
using Product.Infrastructure.Implementations.Account;
using Product.Infrastructure.Repositories;
using Product.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(options => 
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireClaim("userType", "Administrator");
    });
    
    options.AddPolicy("VendorUser", policy =>
    {
        policy.RequireClaim("userType", "VendorUser", "Administrator");
    });
    
    options.AddPolicy("OperatorUser", policy =>
    {
        policy.RequireClaim("userType",  "OperatorUser", "Administrator");
    });
    
    options.AddPolicy("All", policy =>
    {
        policy.RequireClaim("userType",  "VendorUser", "OperatorUser", "Administrator");
    });
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IInviteRepository, InviteRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorFacilityRepository, VendorFacilityRepository>();
builder.Services.AddScoped<IVendorFacilityServiceRepository, VendorFacilityServiceRepository>();
builder.Services.AddScoped<IVendorUserRepository, VendorUserRepository>();
builder.Services.AddScoped<IOperatorRepository, OperatorRepository>();
builder.Services.AddScoped<IOperatorIndustryRepository, OperatorIndustryRepository>();
builder.Services.AddScoped<IOperatorUserRepository, OperatorUserRepository>();

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IVendFacilityService, VendFacilityService>();
builder.Services.AddScoped<IVendorUserService, VendorUserService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IOperatorIndustryService, OperatorIndustryService>();
builder.Services.AddScoped<IOperatorUserService, OperatorUserService>();
builder.Services.AddScoped<IInviteService, InviteService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserPrincipalService, UserPrincipalService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ClaimsPrincipal>(services => services.GetRequiredService<IHttpContextAccessor>().HttpContext.User);

var app = builder.Build();

app.UseMiddleware<MyExceptionHandlingMiddleware>(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();