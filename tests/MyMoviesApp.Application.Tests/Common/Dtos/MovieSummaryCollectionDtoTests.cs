using FluentAssertions;
using MyMoviesApp.Application.Common.Dtos;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Tests.Common.Dtos;

public class MovieSummaryCollectionDtoTests
{
    private static MovieSummaryDto MakeDto(int id, List<UserMovieFormat> formats, List<UserMovieDigitalRetailer> retailers) =>
        new(id, $"Movie {id}", new DateOnly(2020, 1, 1), "/poster.jpg", formats, retailers);

    private static UserMovieFormat _dvdFormat = new() { Id = (int)Format.Dvd, Name = "DVD" };
    private static UserMovieFormat _bluRayFormat = new() { Id = (int)Format.BluRay, Name = "Blu-ray" };
    private static UserMovieFormat _bluRay4KFormat = new() { Id = (int)Format.BluRay4K, Name = "Blu-ray 4K" };
    
    private static UserMovieDigitalRetailer _appleTvRetailer = new() { Id = (int)DigitalRetailer.AppleTv, Name = "Apple TV" };
    private static UserMovieDigitalRetailer _moviesAnywhereRetailer = new() { Id = (int)DigitalRetailer.MoviesAnywhere, Name = "Movies Anywhere" };
    private static UserMovieDigitalRetailer _youTubeRetailer = new() { Id = (int)DigitalRetailer.YouTube, Name = "YouTube" };
    
    [Fact]
    public void Ctor_Should_SetTotalCount_MatchingNumberOfMovies()
    {
        var dtos = new[]
        {
            MakeDto(1, [], []),
            MakeDto(2, [], []),
        };

        var collection = new MovieSummaryCollectionDto(dtos);

        collection.TotalOwnedCount.Should().Be(2);
    }

    [Fact]
    public void Ctor_Should_CountDvdMoviesCorrectly()
    {
        var dtos = new[]
        {
            MakeDto(1, [_dvdFormat], []),
            MakeDto(2, [_bluRayFormat], []),
            MakeDto(3, [_dvdFormat, _bluRay4KFormat], []),
        };

        var collection = new MovieSummaryCollectionDto(dtos);

        collection.TotalDvdCount.Should().Be(2);
    }

    [Fact]
    public void Ctor_Should_CountBluRayMoviesCorrectly()
    {
        var dtos = new[]
        {
            MakeDto(1, [_bluRayFormat], []),
            MakeDto(2, [_dvdFormat], []),
            MakeDto(3, [_bluRayFormat, _bluRay4KFormat], []),
        };

        var collection = new MovieSummaryCollectionDto(dtos);

        collection.TotalBluRayCount.Should().Be(2);
    }

    [Fact]
    public void Ctor_Should_CountBluRay4KMoviesCorrectly()
    {
        var dtos = new[]
        {
            MakeDto(1, [_bluRay4KFormat], []),
            MakeDto(2, [_dvdFormat], []),
        };

        var collection = new MovieSummaryCollectionDto(dtos);

        collection.TotalBluRay4KCount.Should().Be(1);
    }

    [Fact]
    public void Ctor_Should_CountDigitalMoviesCorrectly()
    {
        var dtos = new[]
        {
            MakeDto(1, [], [_appleTvRetailer]),
            MakeDto(2, [], []),
            MakeDto(3, [], [_moviesAnywhereRetailer, _youTubeRetailer]),
        };

        var collection = new MovieSummaryCollectionDto(dtos);

        collection.TotalDigitalCount.Should().Be(2);
    }

    [Fact]
    public void Ctor_Should_SetAllCountsToZero_WhenCollectionIsEmpty()
    {
        var collection = new MovieSummaryCollectionDto(Enumerable.Empty<MovieSummaryDto>());

        collection.TotalOwnedCount.Should().Be(0);
        collection.TotalDvdCount.Should().Be(0);
        collection.TotalBluRayCount.Should().Be(0);
        collection.TotalBluRay4KCount.Should().Be(0);
        collection.TotalDigitalCount.Should().Be(0);
    }
}

