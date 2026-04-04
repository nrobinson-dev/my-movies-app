using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMoviesApp.Infrastructure.Data.Models;

namespace MyMoviesApp.Infrastructure.Data;

public static class DataExtensions
{
    public static IServiceCollection AddMyMoviesAppDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("MyMoviesAppDbSource");
        if (string.IsNullOrWhiteSpace(connString))
        {
            throw new InvalidOperationException(
                "Connection string 'MyMoviesAppDbSource' is missing. Configure it in appsettings or env var ConnectionStrings__MyMoviesAppDbSource.");
        }

        services.AddDbContext<MyMoviesAppContext>(options =>
            options.UseSqlite(connString));

        return services;
    }

    public static IHost MigrateDb(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<MyMoviesAppContext>();
        dbContext.Database.Migrate();

        if (!dbContext.Set<DigitalRetailerDb>().Any())
        {
            dbContext.Set<DigitalRetailerDb>().AddRange(
                new DigitalRetailerDb { Name = "MoviesAnywhere" },
                new DigitalRetailerDb { Name = "AppleTV" },
                new DigitalRetailerDb { Name = "Fandango At Home" },
                new DigitalRetailerDb { Name = "YouTube" },
                new DigitalRetailerDb { Name = "Amazon Prime" }
            );
        }

        if (!dbContext.Set<MovieFormatDb>().Any())
        {
            dbContext.Set<MovieFormatDb>().AddRange(
                new MovieFormatDb { Name = "DVD" },
                new MovieFormatDb { Name = "Blu-Ray" },
                new MovieFormatDb { Name = "4K Blu-Ray" }
            );
        }

        dbContext.SaveChanges();

        return host;
    }

}
