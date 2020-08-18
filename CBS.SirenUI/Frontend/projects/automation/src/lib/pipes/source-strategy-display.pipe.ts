import { Pipe, PipeTransform } from '@angular/core';
import { SourceStrategyTypes } from '../interfaces/interfaces';

@Pipe({
  name: 'sourceStrategyDisplay'
})
export class SourceStrategyDisplayPipe implements PipeTransform {

  transform(value: any): any {
    switch (SourceStrategyTypes[value]) {
      case SourceStrategyTypes.MediaSource:
        return "Media";
    }
  }

}
