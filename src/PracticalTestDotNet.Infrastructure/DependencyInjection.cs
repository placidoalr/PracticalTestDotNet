using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PracticalTestDotNet.Application.Interfaces;
using PracticalTestDotNet.Domain.Interfaces;
using PracticalTestDotNet.Infrastructure.Authentication;
using PracticalTestDotNet.Infrastructure.Persistence;
using PracticalTestDotNet.Infrastructure.Repositories;

namespace PracticalTestDotNet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        services.AddAuthorization();

        return services;
    }
}
