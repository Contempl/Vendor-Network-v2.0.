using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Infrastructure.Dependency_Injection;
using Product.Infrastructure.Extensions;
using Product.Infrastructure.Implementations;
using Product.Infrastructure.Implementations.Account;
using Product.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddDataAccessLayer(builder.Configuration);
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



builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddHttpContextAccessor();
// builder.Services.AddScoped<IUrlHelper, UrlHelper>();

builder.Services.AddScoped<IEmailService, EmailService>(provider =>
{
    var urlHelperFactory = provider.GetRequiredService<IUrlHelperFactory>();
    var actionContextAccessor = provider.GetRequiredService<IActionContextAccessor>();
    var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
    return new EmailService(builder.Configuration ,urlHelper);
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserPrincipalService, UserPrincipalService>();
builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    redisOptions.Configuration = builder.Configuration.GetConnectionString("Redis");
    redisOptions.InstanceName = "Entity_";
});

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