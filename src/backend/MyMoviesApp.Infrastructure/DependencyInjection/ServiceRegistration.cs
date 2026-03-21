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
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }

    private static IServiceCollection AddTmdbServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TmdbOptions>(configuration.GetSection(TmdbOptions.SectionName));

        services.PostConfigure<TmdbOptions>(options =>
        {
            // Try to get the bearer token from environment variable first, then fall back to configuration
            var envToken = Environment.GetEnvironmentVariable("TMDB_ACCESS_TOKEN");
            if (!string.IsNullOrWhiteSpace(envToken))
                options.BearerToken = envToken;
        });

        services.AddOptions<TmdbOptions>().ValidateDataAnnotations().ValidateOnStart();
        
        services.AddHttpClient<ITmdbService, TmdbService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TmdbOptions>>().Value;

            client.BaseAddress = new Uri(options.ApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.BearerToken);
        });

        return services;
    }
    
    private static IServiceCollection AddSecurityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.PostConfigure<JwtOptions>(options =>
        {
            var envKey = Environment.GetEnvironmentVariable("MYMOVIESAPP_JWT_SIGNING_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
                options.SigningKey = envKey;
        });
        
        services.AddOptions<JwtOptions>().ValidateDataAnnotations().ValidateOnStart();
        
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var sp = services.BuildServiceProvider();
                var jwtOptions = sp.GetRequiredService<IOptions<JwtOptions>>().Value;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey!)),
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