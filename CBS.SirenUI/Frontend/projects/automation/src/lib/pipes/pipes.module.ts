import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FeatureTypeDisplayPipe } from './feature-type-display.pipe'
import { TimingStrategyDisplayPipe } from './timing-strategy-display.pipe';
import { PlayoutStrategyDisplayPipe } from './playout-strategy-display.pipe';
import { SourceStrategyDisplayPipe } from './source-strategy-display.pipe';
import { TimeStringDisplayPipe } from './time-string-display.pipe';

@NgModule({
  declarations: [
    FeatureTypeDisplayPipe,
    TimingStrategyDisplayPipe,
    PlayoutStrategyDisplayPipe,
    SourceStrategyDisplayPipe,
    TimeStringDisplayPipe
  ],
  imports: [
    CommonModule
  ],
  exports: [
    FeatureTypeDisplayPipe,
    TimingStrategyDisplayPipe,
    PlayoutStrategyDisplayPipe,
    SourceStrategyDisplayPipe,
    TimeStringDisplayPipe
  ]
})
export class PipesModule {}
