import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ComponentsModule } from './components/components.module';
import { PipesModule } from './pipes/pipes.module';
import { TranmissionlistEventsListComponent } from './components/tranmissionlist-events-list/tranmissionlist-events-list.component';
import { ChannelListComponent } from './components/channel-list/channel-list.component';

const routes: Routes = [
  { path: "channels", component: ChannelListComponent },
  // { path: "channels/:itemId", component: TranmissionlistEventsListComponent },
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
