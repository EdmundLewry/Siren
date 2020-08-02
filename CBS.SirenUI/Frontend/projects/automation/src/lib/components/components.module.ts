import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';

import { TransmissionlistListComponent } from './transmissionlist-list/transmissionlist-list.component';
import { TranmissionlistEventsListComponent } from './tranmissionlist-events-list/tranmissionlist-events-list.component';

@NgModule({
  declarations: [
    TransmissionlistListComponent,
    TranmissionlistEventsListComponent
  ],
  imports: [
    RouterModule,
    MatTableModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    MatButtonModule,
    MatToolbarModule
  ]
})
export class ComponentsModule { }
