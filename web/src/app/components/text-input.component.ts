import { Component, computed, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';

type Size = 'sm' | 'md';

@Component({
  selector: 'app-text-input',
  standalone: true,
  imports: [FormsModule],
  template: `
    <input
      [type]="type()"
      [(ngModel)]="value"
      [placeholder]="placeholder()"
      [attr.min]="min()"
      [attr.max]="max()"
      [attr.step]="step()"
      [class]="classes()" />
  `,
})
export class TextInputComponent {
  readonly value = model<string | number>('');
  readonly type = input<'text' | 'number'>('text');
  readonly placeholder = input('');
  readonly min = input<number | null>(null);
  readonly max = input<number | null>(null);
  readonly step = input<number | string | null>(null);
  readonly size = input<Size>('md');
  readonly fullWidth = input(true);

  readonly classes = computed(() => {
    const base = 'border border-gray-300 rounded-md text-sm text-gray-900 bg-white focus:outline-none focus:ring-2 focus:ring-gray-900 focus:border-transparent';
    const sizeClass = this.size() === 'sm' ? 'h-8 px-2.5' : 'h-9 px-3';
    const numAppearance = this.type() === 'number'
      ? '[appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none'
      : '';
    const width = this.fullWidth() ? 'w-full' : '';
    return `${base} ${sizeClass} ${numAppearance} ${width}`;
  });
}
