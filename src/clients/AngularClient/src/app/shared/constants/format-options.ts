import { FormatId } from "../../movies/models/lookup";
import { PlatformOption } from "../../shared/models/platform-option";

export const FORMAT_OPTIONS: PlatformOption[] = [
    { value: FormatId.Dvd, label: 'DVD', image: "images/formats/dvd-icon.svg", isOwned: false },
    { value: FormatId.BluRay, label: 'Blu-ray', image: "images/formats/blu-ray-icon.svg", isOwned: false },
    { value: FormatId.BluRay4K, label: 'Ultra HD Blu-ray', image: "images/formats/uhd-icon.svg", isOwned: false }
];