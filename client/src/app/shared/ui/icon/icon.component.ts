import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type IconName =
  | 'plus'
  | 'check'
  | 'trash'
  | 'chevron-up'
  | 'chevron-down'
  | 'chevron-left'
  | 'arrow-back'
  | 'calendar'
  | 'text'
  | 'hash'
  | 'checkbox'
  | 'list'
  | 'eye'
  | 'search'
  | 'alert-circle'
  | 'document-plus'
  | 'copy';

/** Inline stroke icons on a 24x24 grid. `flip` mirrors direction-sensitive icons in RTL. */
@Component({
  selector: 'app-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display:inline-flex;line-height:0' },
  template: `
    <svg
      [attr.width]="size()"
      [attr.height]="size()"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      [attr.stroke-width]="strokeWidth()"
      stroke-linecap="round"
      stroke-linejoin="round"
      [class.rtl-flip]="flip()"
      aria-hidden="true"
    >
      @switch (name()) {
        @case ('plus') {
          <path d="M12 5v14M5 12h14" />
        }
        @case ('check') {
          <path d="M20 6 9 17l-5-5" />
        }
        @case ('trash') {
          <path d="M3 6h18M8 6V4h8v2M19 6l-1 14H6L5 6" />
        }
        @case ('chevron-up') {
          <path d="m18 15-6-6-6 6" />
        }
        @case ('chevron-down') {
          <path d="m6 9 6 6 6-6" />
        }
        @case ('chevron-left') {
          <path d="m9 18 6-6-6-6" />
        }
        @case ('arrow-back') {
          <path d="M19 12H5M12 19l-7-7 7-7" />
        }
        @case ('calendar') {
          <rect x="3" y="4" width="18" height="18" rx="2" />
          <path d="M16 2v4M8 2v4M3 10h18" />
        }
        @case ('text') {
          <path d="M4 7V5h16v2M9 19h6M12 5v14" />
        }
        @case ('hash') {
          <path d="M4 9h16M4 15h16M10 3 8 21M16 3l-2 18" />
        }
        @case ('checkbox') {
          <rect x="3" y="3" width="18" height="18" rx="2" />
          <path d="m8 12 3 3 5-6" />
        }
        @case ('list') {
          <path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01" />
        }
        @case ('eye') {
          <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z" />
          <circle cx="12" cy="12" r="3" />
        }
        @case ('search') {
          <circle cx="11" cy="11" r="7" />
          <path d="m21 21-4.3-4.3" />
        }
        @case ('alert-circle') {
          <circle cx="12" cy="12" r="10" />
          <path d="M12 8v5M12 16h.01" />
        }
        @case ('document-plus') {
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
          <path d="M14 2v6h6M12 12v6M9 15h6" />
        }
        @case ('copy') {
          <rect x="9" y="9" width="13" height="13" rx="2" />
          <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
        }
      }
    </svg>
  `,
})
export class IconComponent {
  readonly name = input.required<IconName>();
  readonly size = input(16);
  readonly strokeWidth = input(2);
  readonly flip = input(false);
}
