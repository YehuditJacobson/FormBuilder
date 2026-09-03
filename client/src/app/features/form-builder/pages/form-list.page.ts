import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IconComponent } from '../../../shared/ui/icon/icon.component';
import { TemplateStatus } from '../../../core/models/form-template.models';
import { FormListStore } from '../state/form-list.store';

@Component({
  selector: 'app-form-list-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, DatePipe, IconComponent],
  styles: `
    :host {
      display: block;
    }
    .page {
      max-width: 1120px;
      margin: 0 auto;
      padding: 28px 28px 60px;
    }
    .head {
      display: flex;
      align-items: flex-end;
      justify-content: space-between;
      gap: 20px;
      margin-bottom: 20px;
    }
    .head h1 {
      font-size: 22px;
      font-weight: 700;
    }
    .head .muted {
      font-size: 13px;
      margin-top: 4px;
    }
    table {
      width: 100%;
      border-collapse: collapse;
    }
    thead th {
      text-align: right;
      font-size: 12px;
      font-weight: 700;
      color: var(--text-3);
      padding: 11px 18px;
      background: var(--surface-2);
      border-bottom: 1px solid var(--border);
    }
    tbody td {
      padding: 14px 18px;
      border-bottom: 1px solid var(--border);
      font-size: 13.5px;
      vertical-align: middle;
    }
    tbody tr:last-child td {
      border-bottom: 0;
    }
    tbody tr:hover {
      background: var(--surface-2);
    }
    .name {
      font-weight: 700;
      color: var(--accent-hover);
    }
    .count {
      font-variant-numeric: tabular-nums;
      color: var(--text-2);
    }
    .chev {
      color: var(--text-3);
    }
    .state {
      padding: 48px 24px;
      text-align: center;
      color: var(--text-3);
    }
    .empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      padding: 64px 32px;
    }
    .empty__icon {
      width: 52px;
      height: 52px;
      border-radius: 14px;
      background: var(--accent-tint);
      color: var(--accent-hover);
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 18px;
    }
    .empty h2 {
      font-size: 17px;
      font-weight: 700;
    }
    .empty p {
      color: var(--text-3);
      font-size: 13.5px;
      margin-top: 7px;
      max-width: 400px;
    }
    .empty .btn {
      margin-top: 20px;
    }
    .retry {
      margin-top: 12px;
    }
  `,
  template: `
    <div class="page">
      <div class="head">
        <div>
          <h1>תבניות טפסים</h1>
          <p class="muted">טפסים ארגוניים ומסלול האישורים שהוגדר לכל אחד מהם.</p>
        </div>
        <a class="btn btn--primary" routerLink="/forms/new">
          <app-icon name="plus" [size]="15" [strokeWidth]="2.2" />
          טופס חדש
        </a>
      </div>

      @if (store.loading()) {
        <div class="card state">טוען…</div>
      } @else if (store.error(); as error) {
        <div class="card state">
          <p>{{ error.title }}</p>
          <button type="button" class="btn retry" (click)="store.refresh()">נסה שוב</button>
        </div>
      } @else if (store.isEmpty()) {
        <div class="card empty">
          <div class="empty__icon"><app-icon name="document-plus" [size]="26" [strokeWidth]="1.8" /></div>
          <h2>עדיין אין תבניות טפסים</h2>
          <p>צרו את הטופס הארגוני הראשון, הוסיפו את השדות שלו, והגדירו את מסלול האישורים שהוא צריך לעבור.</p>
          <a class="btn btn--primary" routerLink="/forms/new">
            <app-icon name="plus" [size]="15" [strokeWidth]="2.2" />
            טופס חדש
          </a>
        </div>
      } @else {
        <div class="card">
          <table>
            <thead>
              <tr>
                <th style="width: 34%">שם</th>
                <th>שדות</th>
                <th>שלבי אישור</th>
                <th>סטטוס</th>
                <th>נוצר על ידי</th>
                <th>נוצר</th>
                <th style="width: 36px"></th>
              </tr>
            </thead>
            <tbody>
              @for (template of store.templates(); track template.id) {
                <tr [routerLink]="['/forms', template.id]" style="cursor: pointer">
                  <td><span class="name">{{ template.name }}</span></td>
                  <td class="count">{{ template.fieldCount }}</td>
                  <td class="count">{{ template.approvalStepCount }}</td>
                  <td>
                    <span class="badge" [class.badge--published]="template.status === 'Published'"
                          [class.badge--draft]="template.status === 'Draft'">
                      {{ statusLabel(template.status) }}
                    </span>
                  </td>
                  <td dir="auto">{{ template.createdBy }}</td>
                  <td class="muted" dir="ltr" style="text-align: right">
                    {{ template.createdAtUtc | date: 'dd/MM/yyyy' }}
                  </td>
                  <td><app-icon class="chev rtl-flip" name="chevron-left" [size]="16" /></td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `,
})
export class FormListPage implements OnInit {
  protected readonly store = inject(FormListStore);

  ngOnInit(): void {
    this.store.load();
  }

  protected statusLabel(status: TemplateStatus): string {
    return status === 'Published' ? 'פורסם' : 'טיוטה';
  }
}
