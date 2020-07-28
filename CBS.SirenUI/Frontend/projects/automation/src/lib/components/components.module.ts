import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { MatTableModule } from '@angular/material/table';

import { TransmissionlistListComponent } from './transmissionlist-list/transmissionlist-list.component';

@NgModule({
  declarations: [
    TransmissionlistListComponent
  ],
  imports: [
    RouterModule,
    MatTableModule
  ]
})
export class ComponentsModule { }
