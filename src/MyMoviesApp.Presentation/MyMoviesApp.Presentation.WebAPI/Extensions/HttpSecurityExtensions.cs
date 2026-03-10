namespace MyMoviesApp.Presentation.WebAPI.Extensions;

public static class HttpSecurityExtensions
{
    public static IServiceCollection AddHttpSecurity(this IServiceCollection services)
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
        
        return services;
    }
}