import { TransmissionListEvent } from "./interfaces";

export interface TransmissionListDetails {
    id: number;
    playlistId: number;
    events: TransmissionListEvent[];
    listState: string;
    currentEventId?: number;
}
