export enum FormatId {
    Dvd = 1,
    BluRay = 2,
    BluRay4K = 3
}

export enum DigitalRetailerId {
    MoviesAnywhere = 1,
    AppleTv = 2,
    FandangoAtHome = 3,
    YouTube = 4,
    AmazonPrimeVideo = 5
}

export interface Format {
    id: FormatId;
    name: string;
}

export interface DigitalRetailer {
    id: DigitalRetailerId;
    name: string;
}