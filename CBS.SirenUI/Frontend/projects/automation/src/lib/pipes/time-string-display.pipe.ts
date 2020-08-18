import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timeStringDisplay'
})
export class TimeStringDisplayPipe implements PipeTransform {

  transform(value: string): string {
    return value.replace("T", " ");
  }

}
