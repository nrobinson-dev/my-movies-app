using MyMoviesApp.Application.Common.Models;
using MyMoviesApp.Domain.Enums;

namespace MyMoviesApp.Application.Tests.Common;

public static class TestConstants
{
    public static class Formats
    {
        public static readonly Format Dvd = Format.Dvd;
        public static readonly Format BluRay = Format.BluRay;
        public static readonly Format BluRay4K = Format.BluRay4K;

        public static readonly UserMovieFormatItem DvdItem = new() { Id = (int)Format.Dvd, Name = Format.Dvd.ToString() };
        public static readonly UserMovieFormatItem BluRayItem = new() { Id = (int)Format.BluRay, Name = Format.BluRay.ToString() };
        public static readonly UserMovieFormatItem BluRay4KItem = new() { Id = (int)Format.BluRay4K, Name = Format.BluRay4K.ToString() };
    }

    public static class Retailers
    {
        public static readonly DigitalRetailer AppleTv = DigitalRetailer.AppleTv;
        public static readonly DigitalRetailer MoviesAnywhere = DigitalRetailer.MoviesAnywhere;
        public static readonly DigitalRetailer YouTube = DigitalRetailer.YouTube;

        public static readonly UserMovieDigitalRetailerItem AppleTvItem = new() { Id = (int)DigitalRetailer.AppleTv, Name = DigitalRetailer.AppleTv.ToString() };
        public static readonly UserMovieDigitalRetailerItem MoviesAnywhereItem = new() { Id = (int)DigitalRetailer.MoviesAnywhere, Name = DigitalRetailer.MoviesAnywhere.ToString() };
        public static readonly UserMovieDigitalRetailerItem YouTubeItem = new() { Id = (int)DigitalRetailer.YouTube, Name = DigitalRetailer.YouTube.ToString() };
    }

    public static class Pagination
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 20;
    }
}