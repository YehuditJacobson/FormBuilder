import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { IconComponent } from '../../../shared/ui/icon/icon.component';
import { BuilderValue } from '../model/builder-form';
import { APPROVAL_ACTION_LABELS } from '../model/labels';

/** Read-only rendering of the form being built, updated live from the builder value. */
@Component({
  selector: 'app-form-preview',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [IconComponent],
  styles: `
    .title {
      font-weight: 700;
      font-size: 15px;
      margin-bottom: 14px;
    }
    .placeholder {
      color: var(--text-3);
      font-size: 13px;
    }
    .fields {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }
    .prev-input {
      background: var(--surface-2);
      border: 1px solid var(--border);
      border-radius: var(--radius-sm);
      min-height: 36px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 11px;
      color: var(--text-3);
      font-size: 13px;
    }
    .prev-check {
      display: flex;
      align-items: center;
      gap: 8px;
      color: var(--text-3);
      font-size: 13px;
    }
    .prev-check span {
      width: 15px;
      height: 15px;
      border: 1px solid var(--border-strong);
      border-radius: 3px;
      display: inline-block;
    }
    .submit {
      width: 100%;
      margin-top: 16px;
      opacity: 0.55;
      cursor: default;
    }
    .divider {
      height: 1px;
      background: var(--border);
      border: 0;
      margin: 18px 0;
    }
    .stepper {
      display: flex;
      flex-direction: column;
      gap: 13px;
    }
    .step {
      display: flex;
      gap: 11px;
      align-items: flex-start;
    }
    .step__dot {
      flex-shrink: 0;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 23px;
      height: 23px;
      border-radius: 999px;
      background: var(--accent);
      color: #fff;
      font-size: 12px;
      font-weight: 600;
      font-variant-numeric: tabular-nums;
    }
    .step__name {
      font-weight: 600;
      font-size: 13px;
    }
  `,
  template: `
    <div class="title">{{ value().name || 'ללא שם' }}</div>

    @if (value().fields.length === 0) {
      <p class="placeholder">הוסיפו שדות כדי לראות תצוגה מקדימה.</p>
    } @else {
      <div class="fields">
        @for (field of value().fields; track $index) {
          <div>
            <label class="field-label">
              {{ field.label || 'שדה ללא תווית' }}
              @if (field.isRequired) {
                <span class="field-required">*</span>
              }
            </label>
            @switch (field.fieldType) {
              @case ('Date') {
                <div class="prev-input">
                  יום / חודש / שנה
                  <app-icon name="calendar" [size]="15" />
                </div>
              }
              @case ('Checkbox') {
                <div class="prev-check"><span></span>{{ field.placeholder || 'כן / לא' }}</div>
              }
              @case ('Dropdown') {
                <div class="prev-input">
                  בחר/י
                  <app-icon name="chevron-down" [size]="15" />
                </div>
              }
              @default {
                <div class="prev-input">{{ field.placeholder || '' }}</div>
              }
            }
          </div>
        }
      </div>
      <button type="button" class="btn btn--primary submit" disabled>שליחת הבקשה</button>
    }

    <hr class="divider" />

    <div class="field-label" style="margin-bottom: 12px">מסלול אישורים</div>
    @if (value().approvalSteps.length === 0) {
      <p class="placeholder">הוסיפו שלבי אישור.</p>
    } @else {
      <div class="stepper">
        @for (step of value().approvalSteps; track $index) {
          <div class="step">
            <span class="step__dot">{{ $index + 1 }}</span>
            <div>
              <div class="step__name">{{ step.name || 'שלב ללא שם' }}</div>
              <span class="tag tag--accent" style="height: 20px; margin-top: 5px">
                {{ actionLabel(step.actionType) }}
              </span>
            </div>
          </div>
        }
      </div>
    }
  `,
})
export class FormPreviewComponent {
  readonly value = input.required<BuilderValue>();

  protected actionLabel(action: BuilderValue['approvalSteps'][number]['actionType']): string {
    return APPROVAL_ACTION_LABELS[action];
  }
}
