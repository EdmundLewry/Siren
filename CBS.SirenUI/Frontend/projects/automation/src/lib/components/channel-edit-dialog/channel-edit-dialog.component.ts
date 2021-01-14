import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import {MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { Channel, ChannelCreationData } from '../../interfaces/interfaces';

@Component({
  selector: 'lib-channel-edit-dialog',
  templateUrl: './channel-edit-dialog.component.html',
  styleUrls: ['./channel-edit-dialog.component.css']
})
export class ChannelEditDialog {
  public readonly channelForm: FormGroup;
  public readonly channelNameControl: FormControl;

  public isUpdating = false;
  
  constructor(public dialogRef: MatDialogRef<ChannelEditDialog>,
    @Inject(MAT_DIALOG_DATA) public data: Channel) {

      this.isUpdating = data != null;
      this.channelNameControl = new FormControl("");

      this.channelForm = new FormGroup({
        channelName: this.channelNameControl
      });
  }

  public onNoClick(): void {
    this.dialogRef.close();
  }

  public getResult() {
    return {
      name: this.channelNameControl.value
    } as ChannelCreationData;
  }

  public handleSubmit() {
    this.dialogRef.close();
  }
}
