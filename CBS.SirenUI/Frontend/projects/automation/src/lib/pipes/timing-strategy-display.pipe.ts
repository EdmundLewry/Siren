import { Pipe, PipeTransform } from '@angular/core';
import { TimingStrategyTypes } from '../interfaces/interfaces';

@Pipe({
  name: 'timingStrategyDisplay'
})
export class TimingStrategyDisplayPipe implements PipeTransform {

  transform(value: any): any {
    switch (TimingStrategyTypes[value]) {
      case TimingStrategyTypes.Fixed:
        return "Fixed";
      case TimingStrategyTypes.Sequential:
        return "Sequential";
    }
  }

}
