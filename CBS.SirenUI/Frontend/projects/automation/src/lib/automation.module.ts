import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ComponentsModule } from './components/components.module';
import { PipesModule } from './pipes/pipes.module';
import { TranmissionlistEventsListComponent } from './components/tranmissionlist-events-list/tranmissionlist-events-list.component';

const routes: Routes = [
  { path: "transmissionlist/:itemId", component: TranmissionlistEventsListComponent }
]

@NgModule({
  declarations: [],
  imports: [
    ComponentsModule,
    PipesModule,
    RouterModule.forChild(routes)
  ],
  exports: [
    ComponentsModule,
    PipesModule
  ]
})
export class AutomationModule { }
