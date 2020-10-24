import { TimingStrategy } from "./itiming-strategy";
import { TransmissionListEventFeature } from "./itransmission-list-event-feature";

export interface TransmissionListEventDetails {
    id: number;
    eventState: string;
    expectedDuration: string;
    expectedStartTime: string;
    actualDuration: string;
    actualStartTime: string;
    relatedDeviceListEventCount: number;
    eventTimingStrategy: TimingStrategy;
    eventFeatures: TransmissionListEventFeature[];
    relatedPlaylistEventId: number;
}
