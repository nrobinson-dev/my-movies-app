import { Format, DigitalRetailer } from "./lookup";

export interface MovieSummary {
    tmdbId: number;
    title: string;
    releaseDate: string;
    posterPath: string | null;
    formats: Format[];
    digitalRetailers: DigitalRetailer[];
}

export interface MovieSummaryCollection {
    movies: MovieSummary[];
    totalDvdCount: number;
    totalBluRayCount: number;
    totalBluRay4KCount: number;
    totalDigitalCount: number;
    page: number;
    totalPages: number;
    totalResults: number;
}