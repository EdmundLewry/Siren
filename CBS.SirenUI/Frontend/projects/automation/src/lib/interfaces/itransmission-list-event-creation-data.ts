import { MediaSourceStrategy } from "./imedia-source-strategy";
import { PlayoutStrategy } from "./iplayout-strategy";
import { TimingStrategy } from "./itiming-strategy";
import { RelativePosition } from "./relative-position.enum";

export interface TransmissionListEventFeatureCreationData {
  uid?: string;
  featureType: string;
  playoutStrategy: PlayoutStrategy;
  sourceStrategy: MediaSourceStrategy;
  duration: string;
}

export interface ListPositionData{
  associatedEventId: number;
  relativePosition: RelativePosition;
}

export interface TransmissionListEventCreationData {
  timingData: TimingStrategy;
  features: TransmissionListEventFeatureCreationData[];
  listPosition: ListPositionData;
}
