using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using MyMoviesApp.Infrastructure.Configuration;

namespace MyMoviesApp.Infrastructure.Services;

/// <summary>
/// Delegating handler that injects the TMDB bearer token on each outgoing request,
/// reading it fresh from TmdbOptions so a token change takes effect on the next request
/// after a restart without requiring redeployment.
/// </summary>
public class TmdbAuthHandler(IOptionsMonitor<TmdbOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.CurrentValue.BearerToken);
        return base.SendAsync(request, cancellationToken);
    }
}

