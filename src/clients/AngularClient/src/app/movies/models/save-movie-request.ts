export type SaveMovieRequest = {
    tmdbId: number;
    title: string;
    releaseDate: string;
    posterPath: string | null;
    formats: number[];
    digitalRetailers: number[];
}