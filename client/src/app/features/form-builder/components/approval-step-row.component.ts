import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { IconComponent } from '../../../shared/ui/icon/icon.component';
import { StepGroup } from '../model/builder-form';
import { APPROVAL_ACTION_OPTIONS } from '../model/labels';

/** One editable approval step: name, action type, approver, reorder and remove. */
@Component({
  selector: 'app-approval-step-row',
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
    .kicker {
      font-size: 12px;
      font-weight: 600;
      color: var(--text-3);
    }
    .spacer {
      flex: 1;
    }
    .grid {
      display: grid;
      grid-template-columns: 1.3fr 1fr 1.15fr;
      gap: 12px;
    }
    @media (max-width: 720px) {
      .grid {
        grid-template-columns: 1fr;
      }
    }
  `,
  template: `
    <div class="row" [class.is-invalid]="showError()" [formGroup]="group()">
      <div class="row__bar">
        <span class="order">{{ index() + 1 }}</span>
        <span class="kicker">שלב אישור</span>
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
          aria-label="הסר שלב"
        >
          <app-icon name="trash" [size]="15" />
        </button>
      </div>

      <div class="grid">
        <div>
          <label class="field-label">שם השלב <span class="field-required">*</span></label>
          <input
            class="input"
            formControlName="name"
            [class.is-invalid]="showError()"
            placeholder="לדוגמה: אישור מנהל ישיר"
          />
          @if (showError()) {
            <div class="field-error">
              <app-icon name="alert-circle" [size]="13" [strokeWidth]="2.4" />
              שם השלב הוא שדה חובה.
            </div>
          }
        </div>
        <div>
          <label class="field-label">סוג פעולה <span class="field-required">*</span></label>
          <select class="select" formControlName="actionType">
            @for (option of actionOptions; track option.value) {
              <option [value]="option.value">{{ option.label }}</option>
            }
          </select>
        </div>
        <div>
          <label class="field-label">מאשר</label>
          <input class="input" formControlName="approverId" placeholder="שם או אימייל (רשות)" />
        </div>
      </div>
    </div>
  `,
})
export class ApprovalStepRowComponent {
  readonly group = input.required<StepGroup>();
  readonly index = input.required<number>();
  readonly count = input.required<number>();

  readonly remove = output<void>();
  readonly moveUp = output<void>();
  readonly moveDown = output<void>();

  protected readonly actionOptions = APPROVAL_ACTION_OPTIONS;

  protected showError(): boolean {
    const control = this.group().controls.name;
    return control.invalid && control.touched;
  }
}
