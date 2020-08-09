export interface PlayoutStrategy {
  strategyType: string;
}

export interface MediaSourceStrategy {
  strategyType: string;
  som?: string;
  eom?: string;
  mediaName?: string;
}

export interface TransmissionListEventFeatureCreationData {
  featureType: string;
  playoutStrategy: PlayoutStrategy;
  sourceStrategy: MediaSourceStrategy;
}

export interface TransmissionListEventTimingData {
  timingStrategyType: string;
  targetStartTime?: string;
}

export interface TransmissionListEventCreationData {
  timingData: TransmissionListEventTimingData;
  features: TransmissionListEventFeatureCreationData[];
}
