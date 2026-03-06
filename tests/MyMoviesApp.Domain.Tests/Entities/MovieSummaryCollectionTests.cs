using FluentAssertions;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Domain.Tests.Entities;

public class MovieSummaryCollectionTests
{
    private readonly UserMovieFormat _dvdFormat = new() { Id = (int)Format.Dvd, Name = "DVD" };
    private readonly UserMovieFormat _bluRayFormat = new() { Id = (int)Format.BluRay, Name = "Blu-ray" };
    private readonly UserMovieFormat _bluRay4KFormat = new() { Id = (int)Format.BluRay4K, Name = "Blu-ray 4K" };
    private readonly UserMovieDigitalRetailer _appleTvRetailer = new() { Id = (int)DigitalRetailer.AppleTv, Name = "Apple TV" };
    private readonly UserMovieDigitalRetailer _moviesAnywhereRetailer = new() { Id = (int)DigitalRetailer.MoviesAnywhere, Name = "Movies Anywhere" };
    private readonly UserMovieDigitalRetailer _youTubeRetailer = new() { Id = (int)DigitalRetailer.YouTube, Name = "YouTube" };
    
    private static MovieSummary MakeSummary(int id, List<UserMovieFormat> formats, List<UserMovieDigitalRetailer> retailers) =>
        new()
        {
            MovieId = id,
            Title = $"Movie {id}",
            ReleaseDate = new DateOnly(2020, 1, 1),
            PosterPath = $"/{id}.jpg",
            Formats = formats,
            DigitalRetailers = retailers
        };

    [Fact]
    public void Ctor_Should_SetTotalCount_MatchingMovieCount()
    {
        var movies = new List<MovieSummary>
        {
            MakeSummary(1, [], []),
            MakeSummary(2, [], []),
            MakeSummary(3, [], []),
        };

        var collection = new MovieSummaryCollection(movies);

        collection.TotalCount.Should().Be(3);
    }

    [Fact]
    public void Ctor_Should_SetAllCountsToZero_WhenListIsEmpty()
    {
        var collection = new MovieSummaryCollection(new List<MovieSummary>());

        collection.TotalCount.Should().Be(0);
        collection.TotalDvdCount.Should().Be(0);
        collection.TotalBluRayCount.Should().Be(0);
        collection.TotalBluRay4KCount.Should().Be(0);
        collection.TotalDigitalCount.Should().Be(0);
    }

    [Fact]
    public void Ctor_Should_SetAllCountsToZero_WhenListIsNull()
    {
        var collection = new MovieSummaryCollection(null);

        collection.TotalCount.Should().Be(0);
        collection.TotalDvdCount.Should().Be(0);
        collection.TotalBluRayCount.Should().Be(0);
        collection.TotalBluRay4KCount.Should().Be(0);
        collection.TotalDigitalCount.Should().Be(0);
        collection.Movies.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_Should_CountDvdFormatsCorrectly()
    {
        var movies = new List<MovieSummary>
        {
            MakeSummary(1, [_dvdFormat], []),
            MakeSummary(2, [_bluRayFormat], []),
            MakeSummary(3, [_dvdFormat, _bluRay4KFormat], []),
        };

        var collection = new MovieSummaryCollection(movies);

        collection.TotalDvdCount.Should().Be(2);
    }

    [Fact]
    public void Ctor_Should_CountBluRayFormatsCorrectly()
    {
        var movies = new List<MovieSummary>
        {
            MakeSummary(1, [_bluRayFormat], []),
            MakeSummary(2, [_dvdFormat], []),
            MakeSummary(3, [_bluRayFormat, _dvdFormat], []),
        };

        var collection = new MovieSummaryCollection(movies);

        collection.TotalBluRayCount.Should().Be(2);
    }

    [Fact]
    public void Ctor_Should_CountBluRay4KFormatsCorrectly()
    {
        var movies = new List<MovieSummary>
        {
            MakeSummary(1, [_bluRay4KFormat], []),
            MakeSummary(2, [_dvdFormat], []),
        };

        var collection = new MovieSummaryCollection(movies);

        collection.TotalBluRay4KCount.Should().Be(1);
    }

    [Fact]
    public void Ctor_Should_CountDigitalRetailersCorrectly()
    {
        var movies = new List<MovieSummary>
        {
            MakeSummary(1, [], [_appleTvRetailer]),
            MakeSummary(2, [], []),
            MakeSummary(3, [], [_moviesAnywhereRetailer, _youTubeRetailer]),
        };

        var collection = new MovieSummaryCollection(movies);

        collection.TotalDigitalCount.Should().Be(2);
    }

    [Fact]
    public void Movies_Should_BeIndependentCopy_OfInputList()
    {
        var original = new List<MovieSummary> { MakeSummary(1, [], []) };
        var collection = new MovieSummaryCollection(original);

        original.Add(MakeSummary(2, [], []));

        collection.TotalCount.Should().Be(1);
    }
}

