import { Pipe, PipeTransform } from '@angular/core';
import { FeatureTypes } from '../interfaces/feature-types.enum';

@Pipe({
  name: 'featureTypeDisplay'
})
export class FeatureTypeDisplayPipe implements PipeTransform {

  transform(value: any): any {
    switch (FeatureTypes[value]) {
      case FeatureTypes.Video:
        return "Video";
    }
  }

}
