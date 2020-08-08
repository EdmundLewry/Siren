import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormGroup, FormControl } from '@angular/forms';

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

@Component({
  selector: 'lib-create-event-dialog',
  templateUrl: './create-event-dialog.component.html',
  styleUrls: ['./create-event-dialog.component.css']
})
export class CreateEventDialogComponent {
  public readonly transmissionListEventForm: FormGroup;
  public readonly timingStrategyTypeControl: FormControl;
  public readonly targetStartTimeControl: FormControl;
  public readonly featureTypeControl: FormControl;
  public readonly playoutStrategyTypeControl: FormControl;
  public readonly sourceStrategyTypeControl: FormControl;
  public readonly somControl: FormControl;
  public readonly eomControl: FormControl;
  public readonly mediaNameControl: FormControl;


  public readonly timingStrategyTypes: string[] = [
    "Sequential",
    "Fixed"
  ];

  public readonly playoutStrategyTypes: string[] = [
    "Primary Video"
  ];

  public readonly sourceStrategyTypes: string[] = [
    "Media Source"
  ];

  public readonly featureTypes: string[] = [
    "Video"
  ];

  constructor(
    public dialogRef: MatDialogRef<CreateEventDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TransmissionListEventCreationData) {
      this.timingStrategyTypeControl = new FormControl(this.timingStrategyTypes[0]);
      this.targetStartTimeControl = new FormControl();
      this.featureTypeControl = new FormControl(this.featureTypes[0]);
      this.playoutStrategyTypeControl = new FormControl(this.playoutStrategyTypes[0]);
      this.sourceStrategyTypeControl = new FormControl(this.sourceStrategyTypes[0]);
      this.somControl = new FormControl("00:00:00:00");
      this.eomControl = new FormControl("00:00:30:00");
      this.mediaNameControl = new FormControl("TestInstance");

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
        strategyType: formValue.timingStrategyType,
        targetStartTime: formValue.targetStartTime
      },
      features: [{
        featureType: formValue.featureType,
        playoutStrategy: {
          strategyType: formValue.playoutStrategyType
        },
        sourceStrategy: {
          strategyType: formValue.sourceStrategyType,
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
}
