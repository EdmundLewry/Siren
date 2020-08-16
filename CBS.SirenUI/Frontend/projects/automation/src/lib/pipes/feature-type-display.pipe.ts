import { Pipe, PipeTransform } from '@angular/core';
import { FeatureTypes } from '../interfaces/feature-types.enum';

@Pipe({
  name: 'featureTypeDisplay'
})
export class FeatureTypeDisplayPipe implements PipeTransform {

  transform(value: any): any {
    console.log(value);
    console.log(FeatureTypes[value]);
    switch (FeatureTypes[value]) {
      case FeatureTypes.Video:
        return "Video"
    }
  }

}
