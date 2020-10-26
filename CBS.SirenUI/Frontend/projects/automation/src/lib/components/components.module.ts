import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PipesModule } from '../pipes/pipes.module';
import { CommonModule } from '@angular/common';
import { DragDropModule } from '@angular/cdk/drag-drop';


import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';

import { TranmissionlistEventsListComponent } from './tranmissionlist-events-list/tranmissionlist-events-list.component';
import { ConfirmationDialogComponent } from './confirmation-dialog/confirmation-dialog.component'
import { TransmissionListEventEditDialog } from './transmission-list-event-edit-dialog/transmission-list-event-edit-dialog.component';

@NgModule({
  declarations: [
    TranmissionlistEventsListComponent,
    ConfirmationDialogComponent,
    TransmissionListEventEditDialog
  ],
  imports: [
    RouterModule,
    MatTableModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    MatButtonModule,
    MatToolbarModule,
    MatDialogModule,
    MatSelectModule,
    MatDividerModule,
    MatChipsModule,
    FormsModule,
    DragDropModule,
    ReactiveFormsModule,
    MatInputModule,
    PipesModule,
    CommonModule
  ]
})
export class ComponentsModule { }
