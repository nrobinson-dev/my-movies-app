using System;
using FluentAssertions;
using Moq;
using MyMoviesApp.Domain.Entities;
using MyMoviesApp.Domain.Enums;
using Xunit;

namespace MyMoviesApp.Domain.Tests.Entities
{
    public class MovieTests
    {
        private readonly int _tmdbId = 123;
        private readonly string _title = "The Matrix";
        private readonly DateOnly _releaseDate = DateOnly.Parse("1999-03-31");
        private readonly string _posterPath = "/poster.jpg";
        
        private readonly Format _dvdFormat = Format.Dvd;
        private readonly Format _bluRayFormat = Format.BluRay;
        private readonly DigitalRetailer _appleTvRetailer = DigitalRetailer.AppleTv;
        private readonly DigitalRetailer _moviesAnywhereRetailer = DigitalRetailer.MoviesAnywhere;
        
        [Fact]
        public void MovieSummary_Ctor_AssignsAllProperties()
        {
            // Act
            var entity = new MovieSummary
            {
                MovieId = _tmdbId,
                Title = _title,
                ReleaseDate = _releaseDate,
                PosterPath = _posterPath
            };

            // Assert
            entity.MovieId.Should().Be(_tmdbId);
            entity.Title.Should().Be(_title);
            entity.ReleaseDate.Should().Be(_releaseDate);
            entity.PosterPath.Should().Be(_posterPath);
        }

        [Fact]
        public void MovieSummary_Should_AllowFormats()
        {
            // Arrange
            var formats = new List<Format>
            {
                _dvdFormat, 
                _bluRayFormat
            };

            // Act
            var entity = new MovieSummary
            {
                MovieId = _tmdbId,
                Title = _title,
                ReleaseDate = _releaseDate,
                PosterPath = _posterPath
            };

            entity.Formats = formats;
            // Assert
            entity.Formats.Should().Contain(_dvdFormat);
            entity.Formats.Should().Contain(_bluRayFormat);
        }

        [Fact]
        public void MovieSummary_Should_AllowDigitalRetailers()
        {
            // Arrange
            var digitalRetailers = new List<DigitalRetailer>
            {
                _moviesAnywhereRetailer, 
                _appleTvRetailer
            };

            // Act
            var entity = new MovieSummary
            {
                MovieId = _tmdbId,
                Title = _title,
                ReleaseDate = _releaseDate,
                PosterPath = _posterPath
            };

            entity.DigitalRetailers = digitalRetailers;
            
            // Assert
            entity.DigitalRetailers.Should().Contain(_moviesAnywhereRetailer);
            entity.DigitalRetailers.Should().Contain(_appleTvRetailer);
        }
    }
}