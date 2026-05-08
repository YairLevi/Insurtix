import { Component, computed, input, output } from '@angular/core';

type Variant = 'primary' | 'secondary' | 'link';
type Size = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-button',
  standalone: true,
  template: `
    <button
      [type]="type()"
      [disabled]="disabled()"
      [title]="title()"
      [class]="classes()"
      (click)="clicked.emit($event)">
      <ng-content />
    </button>
  `,
})
export class ButtonComponent {
  readonly variant = input<Variant>('secondary');
  readonly size = input<Size>('md');
  readonly type = input<'button' | 'submit'>('button');
  readonly disabled = input(false);
  readonly title = input('');
  readonly fullWidth = input(false);
  readonly extraClass = input('');
  readonly clicked = output<MouseEvent>();

  readonly classes = computed(() => {
    if (this.variant() === 'link') {
      return `text-sm text-gray-500 hover:text-gray-700 transition-colors cursor-pointer ${this.extraClass()}`;
    }
    const base = 'inline-flex items-center justify-center gap-1.5 rounded-md font-medium transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed';
    const sizeClass = {
      sm: 'h-9 px-3 text-xs',
      md: 'h-9 px-4 text-sm',
      lg: 'h-11 px-4 text-sm',
    }[this.size()];
    const variantClass = {
      primary: 'bg-gray-900 text-white hover:bg-gray-700',
      secondary: 'border border-gray-200 bg-white text-gray-700 hover:bg-gray-50',
      link: '',
    }[this.variant()];
    const width = this.fullWidth() ? 'w-full' : '';
    return `${base} ${sizeClass} ${variantClass} ${width} ${this.extraClass()}`;
  });
}
