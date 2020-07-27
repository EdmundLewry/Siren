import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ComponentsModule } from './components/components.module';
import { AutomationComponent } from './components/automation.component';

const routes: Routes = [
  { path: "txlist", component: AutomationComponent }
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
