using Microsoft.EntityFrameworkCore;
using MyMoviesApp.Application.Common.Interfaces;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;
using MyMoviesApp.Infrastructure.Data.Models;
using UserMovieDigitalRetailer = MyMoviesApp.Infrastructure.Data.Models.UserMovieDigitalRetailer;
using UserMovieFormat = MyMoviesApp.Infrastructure.Data.Models.UserMovieFormat;

namespace MyMoviesApp.Infrastructure.Data.Repositories;

public class UserRepository(MyMoviesAppContext dbcontext) : IUserRepository
{
    public async Task<int> CreateUserAsync(User user, string passwordHash, CancellationToken cancellationToken)
    {
        var userDb = new UserDb
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
        dbcontext.Users.Add(userDb);
        
        return await dbcontext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetAuthenticatedUserAsync(string userName, string passwordHash)
    {
        var userDb = await dbcontext.Users.FirstOrDefaultAsync(u => u.Email == userName && u.PasswordHash == passwordHash);
        return userDb is null ? null : new User(userDb.Id, userDb.Email);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        var userDb = await dbcontext.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        return userDb is null ? null : new User(userDb.Id, userDb.Email);
    }

    public async Task<(User user, string passwordHash)?> GetUserWithPasswordHashByEmailAsync(string email, CancellationToken ct)
    {
        var userDb = await dbcontext.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (userDb is null)
            return null;

        var user = new User(userDb.Id, userDb.Email);
        return (user, userDb.PasswordHash);
    }

    // TODO: implement pagination and filtering by format/retailer
    public async Task<MovieSummaryCollection> GetUserMoviesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userMovies = await dbcontext.UserMovies
            .Include(m => m.UserMovieFormats)
            .Include(m => m.UserMovieDigitalRetailers)
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);

        var movies = userMovies.Select(m => new MovieSummary
            {
                MovieId = m.TmdbId,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate,
                PosterPath = m.PosterPath,
                Formats = m.UserMovieFormats
                    .Select(umf => new Domain.Entities.UserMovieFormat
                    {
                        Id = umf.MovieFormatId,
                        Name = ((Format)umf.MovieFormatId).ToString()
                    })
                    .Distinct()
                    .ToList(),
                DigitalRetailers = m.UserMovieDigitalRetailers
                    .Select(r => new Domain.Entities.UserMovieDigitalRetailer
                    {
                        Id = r.DigitalRetailerId,
                        Name = ((Domain.Enums.DigitalRetailer)r.DigitalRetailerId).ToString()
                    })
                    .Distinct()
                    .ToList()
            })
            .Distinct()
            .ToList();

        return new MovieSummaryCollection(movies);
    }
    
    public async Task<UserMovieFormatsAndDigitalRetailers> GetUserMovieFormatsAndDigitalRetailersAsync(Guid userId, int movieId, CancellationToken cancellationToken)
    {
        var formats = await dbcontext.UserMovieFormats
            .Where(f => dbcontext.UserMovies
                .Any(um => um.UserId == userId && um.TmdbId == movieId && um.Id == f.UserMovieId))
            .Select(f => new Domain.Entities.UserMovieFormat()
            {
                Id = f.MovieFormatId,
                Name = ((Format)f.MovieFormatId).ToString()
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var retailers = await dbcontext.UserMovieDigitalRetailers
            .Where(r => dbcontext.UserMovies
                .Any(um => um.UserId == userId && um.TmdbId == movieId && um.Id == r.UserMovieId))
            .Select(r => new Domain.Entities.UserMovieDigitalRetailer()
            {
                Id = r.DigitalRetailerId,
                Name = ((Domain.Enums.DigitalRetailer)r.DigitalRetailerId).ToString()
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        return new UserMovieFormatsAndDigitalRetailers
        {
            Formats = formats,
            DigitalRetailers = retailers
        };
    }

    public async Task<int> SaveUserMovieAsync(Guid userId, SaveMovieSummary movieSummary, CancellationToken cancellationToken)
    {
        await using var transaction = await dbcontext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var movie = await dbcontext.UserMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.TmdbId == movieSummary.MovieId, cancellationToken);
            
            if (movie != null)
            {
                movie.Title = movieSummary.Title;
                movie.ReleaseDate = movieSummary.ReleaseDate;
                movie.PosterPath = movieSummary.PosterPath;
            }
            else
            {
                movie = new UserMovie
                {
                    UserId = userId,
                    TmdbId = movieSummary.MovieId,
                    Title = movieSummary.Title,
                    ReleaseDate = movieSummary.ReleaseDate,
                    PosterPath = movieSummary.PosterPath,
                    CreatedAt = DateTime.UtcNow
                };
                dbcontext.UserMovies.Add(movie);
            }

            await dbcontext.SaveChangesAsync(cancellationToken);

            // Clear existing formats and retailers for update scenarios
            if (movie.Id > 0)
            {
                var existingFormats = dbcontext.UserMovieFormats.Where(f => f.UserMovieId == movie.Id);
                var existingRetailers = dbcontext.UserMovieDigitalRetailers.Where(r => r.UserMovieId == movie.Id);
                
                dbcontext.UserMovieFormats.RemoveRange(existingFormats);
                dbcontext.UserMovieDigitalRetailers.RemoveRange(existingRetailers);
                await dbcontext.SaveChangesAsync(cancellationToken);
            }

            // Add new formats
            foreach (var format in movieSummary.Formats)
            {
                var userMovieFormat = new UserMovieFormat()
                {
                    UserMovieId = movie.Id,
                    MovieFormatId = (int)format
                };
                dbcontext.UserMovieFormats.Add(userMovieFormat);
            }

            // Add new retailers
            foreach (var digitalRetailer in movieSummary.DigitalRetailers)
            {
                var userMovieRetailer = new UserMovieDigitalRetailer()
                {
                    UserMovieId = movie.Id,
                    DigitalRetailerId = (int)digitalRetailer
                };
                dbcontext.UserMovieDigitalRetailers.Add(userMovieRetailer);
            }

            // Save all changes and commit transaction
            var result = await dbcontext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return result;
        }
        catch
        {
            // Rollback happens automatically when transaction is disposed on exception
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task DeleteUserMovieAsync(Guid userId, int tmdbId, CancellationToken cancellationToken)
    {
        var movie = await dbcontext.UserMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.TmdbId == tmdbId, cancellationToken);
        if (movie != null)
        {
            dbcontext.UserMovies.Remove(movie);
            await dbcontext.SaveChangesAsync(cancellationToken);
        }
    }
}
