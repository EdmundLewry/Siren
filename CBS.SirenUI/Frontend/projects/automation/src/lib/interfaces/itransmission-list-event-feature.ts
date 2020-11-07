import { Device } from "./idevice";
import { MediaSourceStrategy } from "./imedia-source-strategy";
import { PlayoutStrategy } from "./iplayout-strategy";

export interface TransmissionListEventFeature {
    uid?: string;
    featureType: string;
    playoutStrategy: PlayoutStrategy;
    sourceStrategy: MediaSourceStrategy;
    device: Device;
    deviceListEventId?: number;
    duration: string;
  }