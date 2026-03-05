using Microsoft.Extensions.DependencyInjection;
using MyMoviesApp.Application.Features.Movies.Queries;
using MyMoviesApp.Application.Features.User.Commands;
using MyMoviesApp.Application.Features.User.Queries;

namespace MyMoviesApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            // Auth
            cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(LoginUserCommand).Assembly);
            
            // Users
            cfg.RegisterServicesFromAssembly(typeof(GetMovieByTmdbMovieIdQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(GetMovieOwnershipQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(GetMovieSearchResultsQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(SaveMovieOwnershipCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(DeleteMovieCommand).Assembly);
        });

        return services;
    }
}