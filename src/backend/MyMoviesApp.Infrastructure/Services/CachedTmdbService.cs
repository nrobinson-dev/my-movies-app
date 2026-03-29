using System.Text.Json;
using MyMoviesApp.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Infrastructure.Services;

public class CachedTmdbService(ITmdbService innerService, IDistributedCache cache, ILogger<CachedTmdbService> logger) : ITmdbService
{
    private static readonly SemaphoreSlim CacheLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    
    public async Task<MovieDetail> GetMovieByTmdbMovieIdAsync(int id, CancellationToken cancellationToken)
    {
        string key = $"movie-{id}";
        
        var options = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };
        
        return await GetOrSetCacheAsync(key, () => innerService.GetMovieByTmdbMovieIdAsync(id, cancellationToken), options, cancellationToken );
    }

    public async Task<MovieSummaryCollection> SearchMoviesAsync(string term, CancellationToken cancellationToken, int page = 1)
    {
        string key = $"movie-search-{Uri.EscapeDataString(term.ToLower().Trim())}-{page}";
        
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(1)
        };
        
        return await GetOrSetCacheAsync(key, () => innerService.SearchMoviesAsync(term, cancellationToken, page), options, cancellationToken );
    }
    
    private async Task<T> GetOrSetCacheAsync<T>(string key, Func<Task<T>> fetchFunction, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
    {
        var cachedData = await TryGetFromCacheAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<T>(cachedData, JsonOptions)!;
        }
        
        await CacheLock.WaitAsync(cancellationToken);
        try
        {
            cachedData = await TryGetFromCacheAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<T>(cachedData, JsonOptions)!;
            }
            
            var data = await fetchFunction();

            if (data != null)
            {
                await TrySetCacheAsync(key, data, options, cancellationToken);
            }

            return data;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache READ failed for key: {Key}", key);
        }
        finally{
            CacheLock.Release();
        }

        return await fetchFunction();
    }
    
    private async Task<string?> TryGetFromCacheAsync(string key, CancellationToken ct)
    {
        try 
        { 
            return await cache.GetStringAsync(key, ct); 
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache READ failed for key: {Key}. Falling back to TMDB API.", key);
            return null;
        }
    }

    private async Task TrySetCacheAsync<T>(string key, T data, DistributedCacheEntryOptions options, CancellationToken ct)
    {
        try 
        { 
            var serialized = JsonSerializer.Serialize(data, JsonOptions);
            await cache.SetStringAsync(key, serialized, options, ct); 
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache WRITE failed for key: {Key}.", key);
        }
    }
    
}