import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormGroup, FormControl } from '@angular/forms';
import { TimingStrategyTypes } from '../../interfaces/timing-strategy-types.enum';
import { PlayoutStrategyTypes } from '../../interfaces/playout-strategy-types.enum';
import { SourceStrategyTypes } from '../../interfaces/source-strategy-types.enum';
import { FeatureTypes } from '../../interfaces/feature-types.enum';
import { TransmissionListEventDetails } from '../../interfaces/itransmission-list-event-details';

@Component({
  selector: 'lib-transmission-list-event-edit-dialog',
  templateUrl: './transmission-list-event-edit-dialog.component.html',
  styleUrls: ['./transmission-list-event-edit-dialog.component.css']
})
export class TransmissionListEventEditDialog {
  public readonly transmissionListEventForm: FormGroup;
  public readonly timingStrategyTypeControl: FormControl;
  public readonly targetStartTimeControl: FormControl;
  public readonly featureTypeControl: FormControl;
  public readonly playoutStrategyTypeControl: FormControl;
  public readonly sourceStrategyTypeControl: FormControl;
  public readonly somControl: FormControl;
  public readonly eomControl: FormControl;
  public readonly mediaNameControl: FormControl;
  public readonly featureDurationControl: FormControl;

  public featureTypes: string[] = Object.keys(FeatureTypes);
  public playoutStrategyTypes: string[] = Object.keys(PlayoutStrategyTypes);
  public sourceStrategyTypes: string[] = Object.keys(SourceStrategyTypes);
  public timingStrategyTypes: string[] = Object.keys(TimingStrategyTypes);

  public isUpdating = false;
  public eventIsPlaying = false;

  constructor(
    public dialogRef: MatDialogRef<TransmissionListEventEditDialog>,
    @Inject(MAT_DIALOG_DATA) public data: TransmissionListEventDetails) {
      
      console.log("Current data: ", data);

      this.isUpdating = data != null;
      this.eventIsPlaying = data?.eventState == 'PLAYING';

      var timingValues: string[] = Object.values(TimingStrategyTypes);
      this.timingStrategyTypeControl = new FormControl({value: !data ? this.timingStrategyTypes[0] : this.timingStrategyTypes[timingValues.indexOf(data.eventTimingStrategy.strategyType)], disabled: this.eventIsPlaying});

      var defaultTargetStart = this.getDefaultTargetStartTimeValue();
      this.targetStartTimeControl = new FormControl({value: !data?.eventTimingStrategy.targetStartTime ? defaultTargetStart : data.eventTimingStrategy.targetStartTime, disabled: this.eventIsPlaying});

      var eventFeature = data?.eventFeatures.length ? data.eventFeatures[0] : null;
      var featureValues: string[] = Object.values(FeatureTypes);
      this.featureTypeControl = new FormControl({value: !eventFeature ? this.featureTypes[0] : this.featureTypes[featureValues.indexOf(eventFeature.featureType)], disabled: this.eventIsPlaying});
      
      var playoutStrategyValues: string[] = Object.values(PlayoutStrategyTypes);
      this.playoutStrategyTypeControl = new FormControl({value: !eventFeature ? this.playoutStrategyTypes[0] : this.playoutStrategyTypes[playoutStrategyValues.indexOf(eventFeature.playoutStrategy.strategyType)], disabled: this.eventIsPlaying});
      
      var sourceStrategyValues: string[] = Object.values(SourceStrategyTypes);
      this.sourceStrategyTypeControl = new FormControl({value: !eventFeature ? this.sourceStrategyTypes[0] : this.sourceStrategyTypes[sourceStrategyValues.indexOf(eventFeature.sourceStrategy.strategyType)], disabled: this.eventIsPlaying});
      this.somControl = new FormControl({value: !eventFeature?.sourceStrategy.som ? "00:00:00:00" : eventFeature.sourceStrategy.som, disabled: this.eventIsPlaying});
      this.eomControl = new FormControl({value: !eventFeature?.sourceStrategy.eom ? "00:00:30:00" : eventFeature.sourceStrategy.eom, disabled: this.eventIsPlaying});
      this.mediaNameControl = new FormControl({value: !eventFeature?.sourceStrategy.mediaName ? "TestInstance" : eventFeature.sourceStrategy.mediaName, disabled: this.eventIsPlaying});
      this.featureDurationControl = new FormControl(!eventFeature ? "00:00:30:00" : eventFeature.duration);

      this.transmissionListEventForm = new FormGroup({
          timingStrategyType: this.timingStrategyTypeControl,
          targetStartTime: this.targetStartTimeControl,
          featureType: this.featureTypeControl,
          playoutStrategyType: this.playoutStrategyTypeControl,
          sourceStrategyType: this.sourceStrategyTypeControl,
          som: this.somControl,
          eom: this.eomControl,
          mediaName: this.mediaNameControl,
          featureDuration: this.featureDurationControl
      });
    }

  public onNoClick(): void {
    this.dialogRef.close();
  }

  public getResult() {
    return {
      timingData: {
        strategyType: TimingStrategyTypes[this.timingStrategyTypeControl.value],
        targetStartTime: this.targetStartTimeControl.value
      },
      features: [{
        uid: this.data?.eventFeatures.length > 0 ? this.data?.eventFeatures[0].uid : null,
        featureType: FeatureTypes[this.featureTypeControl.value],
        playoutStrategy: {
          strategyType: PlayoutStrategyTypes[this.playoutStrategyTypeControl.value]
        },
        sourceStrategy: {
          strategyType: SourceStrategyTypes[this.sourceStrategyTypeControl.value],
          som: this.somControl.value,
          eom: this.eomControl.value,
          mediaName: this.mediaNameControl.value
        },
        duration: this.featureDurationControl.value
      }]
    };
  }

  public handleSubmit() {
    this.dialogRef.close();
  }

  public isUsingFixedTiming(): boolean {
    return TimingStrategyTypes[this.timingStrategyTypeControl.value] == TimingStrategyTypes.Fixed;
  }

  public isUsingMediaSource(): boolean {
    return SourceStrategyTypes[this.sourceStrategyTypeControl.value] == SourceStrategyTypes.MediaSource;
  }

  public getDefaultTargetStartTimeValue(): string {
    return new Date().toISOString().replace(/\..+/, '') + ":00";
  }
}
