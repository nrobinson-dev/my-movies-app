import { DigitalRetailerId } from "../../movies/models/lookup";
import { PlatformOption } from "../../shared/models/platform-option";

export const DIGITAL_RETAILER_OPTIONS: PlatformOption[] = [
    { value: DigitalRetailerId.MoviesAnywhere, label: 'Movies Anywhere', image: "images/digital-retailers/MA-icon.png", isOwned: false },
    { value: DigitalRetailerId.AppleTv, label: 'Apple TV', image: "images/digital-retailers/A-icon.svg", isOwned: false },
    { value: DigitalRetailerId.FandangoAtHome, label: 'Fandango at Home', image: "images/digital-retailers/FAH-icon.svg", isOwned: false },
    { value: DigitalRetailerId.YouTube, label: 'YouTube', image: "images/digital-retailers/YT-icon.svg", isOwned: false },
    { value: DigitalRetailerId.AmazonPrimeVideo, label: 'Amazon Prime Video', image: "images/digital-retailers/APV-icon.svg", isOwned: false }
];