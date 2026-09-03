import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { IconComponent } from '../../../shared/ui/icon/icon.component';
import { FieldGroup } from '../model/builder-form';
import { FIELD_TYPE_LABELS } from '../model/labels';

/** One editable field within the builder: label, placeholder, "required", reorder and remove. */
@Component({
  selector: 'app-field-row',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, IconComponent],
  styles: `
    .row {
      border: 1px solid var(--border);
      border-radius: var(--radius-sm);
      padding: 13px;
      display: flex;
      flex-direction: column;
      gap: 12px;
      background: var(--surface);
    }
    .row.is-invalid {
      border-color: var(--danger-line);
    }
    .row__bar {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .order {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 23px;
      height: 23px;
      border-radius: 999px;
      background: var(--surface-3);
      color: var(--text-2);
      font-size: 12px;
      font-weight: 600;
      font-variant-numeric: tabular-nums;
    }
    .spacer {
      flex: 1;
    }
    .grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 12px;
    }
    .checkbox {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 13px;
      font-weight: 600;
      color: var(--text-2);
      cursor: pointer;
    }
    .checkbox input {
      width: 15px;
      height: 15px;
      accent-color: var(--accent);
    }
    @media (max-width: 640px) {
      .grid {
        grid-template-columns: 1fr;
      }
    }
  `,
  template: `
    <div class="row" [class.is-invalid]="showError()" [formGroup]="group()">
      <div class="row__bar">
        <span class="order">{{ index() + 1 }}</span>
        <span class="tag tag--accent">{{ typeLabel() }}</span>
        <span class="spacer"></span>
        <button
          type="button"
          class="icon-btn"
          [disabled]="index() === 0"
          (click)="moveUp.emit()"
          aria-label="הזז למעלה"
        >
          <app-icon name="chevron-up" [size]="15" />
        </button>
        <button
          type="button"
          class="icon-btn"
          [disabled]="index() === count() - 1"
          (click)="moveDown.emit()"
          aria-label="הזז למטה"
        >
          <app-icon name="chevron-down" [size]="15" />
        </button>
        <button
          type="button"
          class="icon-btn icon-btn--danger"
          (click)="remove.emit()"
          aria-label="הסר שדה"
        >
          <app-icon name="trash" [size]="15" />
        </button>
      </div>

      <div class="grid">
        <div>
          <label class="field-label">תווית <span class="field-required">*</span></label>
          <input
            class="input"
            formControlName="label"
            [class.is-invalid]="showError()"
            placeholder="תווית השדה"
          />
          @if (showError()) {
            <div class="field-error">
              <app-icon name="alert-circle" [size]="13" [strokeWidth]="2.4" />
              תווית היא שדה חובה.
            </div>
          }
        </div>
        <div>
          <label class="field-label">טקסט רמז</label>
          <input class="input" formControlName="placeholder" placeholder="—" />
        </div>
      </div>

      <label class="checkbox">
        <input type="checkbox" formControlName="isRequired" />
        שדה חובה
      </label>
    </div>
  `,
})
export class FieldRowComponent {
  readonly group = input.required<FieldGroup>();
  readonly index = input.required<number>();
  readonly count = input.required<number>();

  readonly remove = output<void>();
  readonly moveUp = output<void>();
  readonly moveDown = output<void>();

  protected readonly typeLabel = computed(() => FIELD_TYPE_LABELS[this.group().controls.fieldType.value]);

  protected showError(): boolean {
    const control = this.group().controls.label;
    return control.invalid && control.touched;
  }
}
