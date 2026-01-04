import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'shortNumber',
  standalone: true
})
export class ShortNumberPipe implements PipeTransform {

  transform(value: number | null | undefined): string {
    if (value == null) return '0';

    if (value < 100_000) return value.toString();
    if (value < 1_000_000) return `${(value / 1_000).toFixed(1).replace(/\.0$/, '')}k`;
    if (value < 1_000_000_000) return `${(value / 1_000_000).toFixed(2).replace(/\.00$/, '')}M`;

    return `${(value / 1_000_000_000).toFixed(2).replace(/\.00$/, '')}B`;
  }
}
