using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Product.Domain.Enum;
using Product.Domain.Settings;
using Product.Infrastructure.Dependency_Injection;
using Product.Infrastructure.Extensions;
using Product.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddAuthorization(options =>
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
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Auth API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",

        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] {} }
    });
});

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