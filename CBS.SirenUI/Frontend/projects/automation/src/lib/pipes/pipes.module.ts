import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { EnumToArrayPipe } from './enum-to-array.pipe';
import { FeatureTypeDisplayPipe } from './feature-type-display.pipe'

@NgModule({
  declarations: [
    EnumToArrayPipe,
    FeatureTypeDisplayPipe
  ],
  imports: [
    CommonModule
  ],
  exports: [
    EnumToArrayPipe,
    FeatureTypeDisplayPipe
  ]
})
export class PipesModule {}
