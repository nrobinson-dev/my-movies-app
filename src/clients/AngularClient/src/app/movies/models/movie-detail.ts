import { Format, DigitalRetailer } from "./lookup";

export interface MovieDetail {
    tmdbId: number;
    title: string;
    releaseDate: string;
    posterPath: string | null;
    backdropPath: string | null;
    runtime: number;
    tagline: string | null;
    overview: string;
    formats: Format[];
    digitalRetailers: DigitalRetailer[];
}