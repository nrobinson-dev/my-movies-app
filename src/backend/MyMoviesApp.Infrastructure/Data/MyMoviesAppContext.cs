using Microsoft.EntityFrameworkCore;
using MyMoviesApp.Infrastructure.Data.Models;

namespace MyMoviesApp.Infrastructure.Data;

public class MyMoviesAppContext(DbContextOptions<MyMoviesAppContext> options) : DbContext(options)
{
    public DbSet<UserDb> Users => Set<UserDb>();
    public DbSet<UserMovieDb> UserMovies => Set<UserMovieDb>();
    public DbSet<MovieFormatDb> MovieFormats => Set<MovieFormatDb>();
    public DbSet<DigitalRetailerDb> DigitalRetailers => Set<DigitalRetailerDb>();
    public DbSet<UserMovieFormat> UserMovieFormats => Set<UserMovieFormat>();
    public DbSet<UserMovieDigitalRetailer> UserMovieDigitalRetailers => Set<UserMovieDigitalRetailer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserDb>(entity =>
        {
            entity.HasKey(user => user.Id);
            
            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(256);
            
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<UserMovieDb>(entity =>
        {
            entity.HasKey(userMovie => userMovie.Id);

            entity
                .HasOne<UserDb>()
                .WithMany()
                .HasForeignKey(userMovie => userMovie.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasIndex(userMovie => new { userMovie.UserId, userMovie.TmdbId })
                .IsUnique();
        });

        modelBuilder.Entity<UserMovieFormat>(entity =>
        {
            entity.HasKey(userMovieFormat => new { userMovieFormat.UserMovieId, userMovieFormat.MovieFormatId });

            entity
                .HasOne<UserMovieDb>()
                .WithMany(um => um.UserMovieFormats)
                .HasForeignKey(userMovieFormat => userMovieFormat.UserMovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<MovieFormatDb>()
                .WithMany()
                .HasForeignKey(userMovieFormat => userMovieFormat.MovieFormatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserMovieDigitalRetailer>(entity =>
        {
            entity.HasKey(userMovieDigitalRetailer => new { userMovieDigitalRetailer.UserMovieId, userMovieDigitalRetailer.DigitalRetailerId });

            entity
                .HasOne<UserMovieDb>()
                .WithMany(um => um.UserMovieDigitalRetailers)
                .HasForeignKey(userMovieDigitalRetailer => userMovieDigitalRetailer.UserMovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<DigitalRetailerDb>()
                .WithMany()
                .HasForeignKey(userMovieDigitalRetailer => userMovieDigitalRetailer.DigitalRetailerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PreventLookupDeletes();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override int SaveChanges()
    {
        PreventLookupDeletes();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        PreventLookupDeletes();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PreventLookupDeletes();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void PreventLookupDeletes()
    {
        if (ChangeTracker.Entries<MovieFormatDb>().Any(entry => entry.State == EntityState.Deleted))
        {
            throw new InvalidOperationException("MovieFormat records are lookup data and cannot be deleted.");
        }

        if (ChangeTracker.Entries<DigitalRetailerDb>().Any(entry => entry.State == EntityState.Deleted))
        {
            throw new InvalidOperationException("DigitalRetailer records are lookup data and cannot be deleted.");
        }
    }
}
