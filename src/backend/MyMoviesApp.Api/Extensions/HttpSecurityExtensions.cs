using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MyMoviesApp.Api.Extensions;

public static class HttpSecurityExtensions
{
    public static IServiceCollection AddHttpProtection(this IServiceCollection services)
    {
        services.AddHttpsRedirection(options =>
        {
            options.HttpsPort = 7184;
            options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
        });

        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
            options.Preload = true;
        });
        
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("login", limiterOptions =>
            {
                limiterOptions.PermitLimit = 3;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
    
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("user", limiterOptions =>
            {
                limiterOptions.PermitLimit = 30;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 5;
            });
        });
        
        return services;
    }
}