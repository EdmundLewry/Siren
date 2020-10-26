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

  public featureTypes: string[] = Object.keys(FeatureTypes);
  public playoutStrategyTypes: string[] = Object.keys(PlayoutStrategyTypes);
  public sourceStrategyTypes: string[] = Object.keys(SourceStrategyTypes);
  public timingStrategyTypes: string[] = Object.keys(TimingStrategyTypes);

  public isUpdating = false;

  constructor(
    public dialogRef: MatDialogRef<TransmissionListEventEditDialog>,
    @Inject(MAT_DIALOG_DATA) public data: TransmissionListEventDetails) {
      
      this.isUpdating = data != null;

      var timingValues: string[] = Object.values(TimingStrategyTypes);
      this.timingStrategyTypeControl = new FormControl(!data ? this.timingStrategyTypes[0] : this.timingStrategyTypes[timingValues.indexOf(data.eventTimingStrategy.strategyType)]);
      this.targetStartTimeControl = new FormControl(!data ? null : data.eventTimingStrategy.targetStartTime);

      var eventFeature = data?.eventFeatures.length ? data.eventFeatures[0] : null;
      var featureValues: string[] = Object.values(FeatureTypes);
      this.featureTypeControl = new FormControl(!eventFeature ? this.featureTypes[0] : this.featureTypes[featureValues.indexOf(eventFeature.featureType)]);
      
      var playoutStrategyValues: string[] = Object.values(PlayoutStrategyTypes);
      this.playoutStrategyTypeControl = new FormControl(!eventFeature ? this.playoutStrategyTypes[0] : this.playoutStrategyTypes[playoutStrategyValues.indexOf(eventFeature.playoutStrategy.strategyType)]);
      
      var sourceStrategyValues: string[] = Object.values(SourceStrategyTypes);
      this.sourceStrategyTypeControl = new FormControl(!eventFeature ? this.sourceStrategyTypes[0] : this.sourceStrategyTypes[sourceStrategyValues.indexOf(eventFeature.sourceStrategy.strategyType)]);
      this.somControl = new FormControl(!eventFeature?.sourceStrategy.som ? "00:00:00:00" : eventFeature.sourceStrategy.som);
      this.eomControl = new FormControl(!eventFeature?.sourceStrategy.eom ? "00:00:30:00" : eventFeature.sourceStrategy.eom);
      this.mediaNameControl = new FormControl(!eventFeature?.sourceStrategy.mediaName ? "TestInstance" : eventFeature.sourceStrategy.mediaName);

      this.transmissionListEventForm = new FormGroup({
          timingStrategyType: this.timingStrategyTypeControl,
          targetStartTime: this.targetStartTimeControl,
          featureType: this.featureTypeControl,
          playoutStrategyType: this.playoutStrategyTypeControl,
          sourceStrategyType: this.sourceStrategyTypeControl,
          som: this.somControl,
          eom: this.eomControl,
          mediaName: this.mediaNameControl
      });
    }

  public onNoClick(): void {
    this.dialogRef.close();
  }

  public getResult() {
    let formValue = this.transmissionListEventForm.value;
    return {
      timingData: {
        strategyType: TimingStrategyTypes[formValue.timingStrategyType],
        targetStartTime: formValue.targetStartTime
      },
      features: [{
        featureType: FeatureTypes[formValue.featureType],
        playoutStrategy: {
          strategyType: PlayoutStrategyTypes[formValue.playoutStrategyType]
        },
        sourceStrategy: {
          strategyType: SourceStrategyTypes[formValue.sourceStrategyType],
          som: formValue.som,
          eom: formValue.eom,
          mediaName: formValue.mediaName
        }
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
}
