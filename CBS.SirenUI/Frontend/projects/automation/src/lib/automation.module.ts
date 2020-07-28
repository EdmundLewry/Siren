import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ComponentsModule } from './components/components.module';
import { TransmissionlistListComponent } from './components/transmissionlist-list/transmissionlist-list.component';

const routes: Routes = [
  { path: "txlist", component: TransmissionlistListComponent }
]

@NgModule({
  declarations: [],
  imports: [
    ComponentsModule,
    RouterModule.forChild(routes)
  ],
  exports: [
    ComponentsModule
  ]
})
export class AutomationModule { }
