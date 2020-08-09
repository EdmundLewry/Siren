import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormGroup, FormControl } from '@angular/forms';
import { TransmissionListEventCreationData } from '../../interfaces/itransmission-list-event-creation-data';
import { TimingStrategyTypes } from '../../interfaces/timing-strategy-types.enum';
import { PlayoutStrategyTypes } from '../../interfaces/playout-strategy-types.enum';
import { SourceStrategyTypes } from '../../interfaces/source-strategy-types.enum';
import { FeatureTypes } from '../../interfaces/feature-types.enum';

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

  public featureTypes = FeatureTypes;

  constructor(
    public dialogRef: MatDialogRef<CreateEventDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TransmissionListEventCreationData) {
      this.timingStrategyTypeControl = new FormControl(TimingStrategyTypes.Sequential);
      this.targetStartTimeControl = new FormControl();
      this.featureTypeControl = new FormControl(FeatureTypes.Video);
      this.playoutStrategyTypeControl = new FormControl(PlayoutStrategyTypes.PrimaryVideo);
      this.sourceStrategyTypeControl = new FormControl(SourceStrategyTypes.MediaSource);
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
