using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Infrastructure.Configuration;
using MyMoviesApp.Infrastructure.Data;
using MyMoviesApp.Infrastructure.Middleware;
using MyMoviesApp.Infrastructure.Services;
using MyMoviesApp.Infrastructure.Data.Repositories;

namespace MyMoviesApp.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTmdbServices(configuration);
        services.AddSecurityInfrastructure(configuration);
        services.AddMyMoviesAppDb(configuration);
        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }

    private static IServiceCollection AddTmdbServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Try to get the bearer token from environment variable first, then fall back to configuration (appsettings or user secrets)
        var bearerToken = Environment.GetEnvironmentVariable("TMDB_ACCESS_TOKEN") ?? configuration.GetSection(TmdbOptions.SectionName).GetValue<string>("BearerToken");
        
        if (string.IsNullOrWhiteSpace(bearerToken))        {
            throw new InvalidOperationException("TMDB Bearer token is not configured. Set the TMDB_ACCESS_TOKEN environment variable or configure it in appsettings/user secrets under TmdbOptions:BearerToken.");
        }
        
        services.Configure<TmdbOptions>(configuration.GetSection(TmdbOptions.SectionName));

        services.AddHttpClient<ITmdbService, TmdbService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TmdbOptions>>().Value;

            bearerToken = Environment.GetEnvironmentVariable("TMDB_ACCESS_TOKEN") ?? options.BearerToken;

            client.BaseAddress = new Uri(options.ApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", bearerToken);
        });

        return services;
    }
    
    private static IServiceCollection AddSecurityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSigningKey = Environment.GetEnvironmentVariable("MYMOVIESAPP_JWT_SIGNING_KEY") ?? configuration.GetSection(JwtOptions.SectionName).GetValue<string>("SigningKey");
        if (string.IsNullOrWhiteSpace(jwtSigningKey))        {
            throw new InvalidOperationException("JWT signing key is not configured. Set the MYMOVIESAPP_JWT_SIGNING_KEY environment variable or configure it in appsettings/user secrets under JwtOptions:SigningKey.");
        }
        
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        
        jwtSigningKey = Environment.GetEnvironmentVariable("MYMOVIESAPP_JWT_SIGNING_KEY") ?? jwtOptions.SigningKey;
        
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.Name = "XSRF-TOKEN";
        });

        return services;
    }
    
    public static IApplicationBuilder UseAntiforgeryTokenMiddleware(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            var tokens = antiforgery.GetAndStoreTokens(context);
            
            context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, 
                new CookieOptions { 
                    HttpOnly = false, 
                    Secure = true, 
                    SameSite = SameSiteMode.Strict 
                });

            await next();
        });
    }
}