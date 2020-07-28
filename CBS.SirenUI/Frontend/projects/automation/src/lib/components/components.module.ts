import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { MatTableModule } from '@angular/material/table';

import { TransmissionlistListComponent } from './transmissionlist-list/transmissionlist-list.component';
import { TranmissionlistEventsListComponent } from './tranmissionlist-events-list/tranmissionlist-events-list.component';

@NgModule({
  declarations: [
    TransmissionlistListComponent,
    TranmissionlistEventsListComponent
  ],
  imports: [
    RouterModule,
    MatTableModule
  ]
})
export class ComponentsModule { }
