import { Pipe, PipeTransform } from '@angular/core';
import { PlayoutStrategyTypes } from '../interfaces/interfaces';

@Pipe({
  name: 'playoutStrategyDisplay'
})
export class PlayoutStrategyDisplayPipe implements PipeTransform {

  transform(value: any): any {
    switch (PlayoutStrategyTypes[value]) {
      case PlayoutStrategyTypes.PrimaryVideo:
        return "Primary Video";
    }
  }

}
