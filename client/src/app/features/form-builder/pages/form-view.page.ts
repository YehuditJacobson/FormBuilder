import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormTemplateApiService } from '../../../core/api/form-template-api.service';
import { ApiError } from '../../../core/errors/api-error';
import { FormTemplateDetail, TemplateStatus } from '../../../core/models/form-template.models';
import { IconComponent } from '../../../shared/ui/icon/icon.component';
import {
  FormPreviewComponent,
  PreviewField,
  PreviewStep,
} from '../components/form-preview.component';

@Component({
  selector: 'app-form-view-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, DatePipe, IconComponent, FormPreviewComponent],
  styles: `
    :host {
      display: block;
    }
    .page {
      max-width: 1080px;
      margin: 0 auto;
      padding: 26px 28px 60px;
    }
    .head {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 8px;
    }
    .head h1 {
      font-size: 21px;
      font-weight: 700;
    }
    .meta {
      color: var(--text-3);
      font-size: 12.5px;
      margin: 0 0 22px 0;
      padding-inline-start: 43px;
    }
    .layout {
      display: grid;
      grid-template-columns: minmax(0, 1fr) 340px;
      gap: 26px;
      align-items: start;
    }
    .state {
      padding: 48px 24px;
      text-align: center;
      color: var(--text-3);
    }
    @media (max-width: 900px) {
      .layout {
        grid-template-columns: 1fr;
      }
    }
  `,
  template: `
    <div class="page">
      @if (loading()) {
        <div class="card state">טוען…</div>
      } @else if (error(); as err) {
        <div class="card state">
          <p>{{ err.status === 404 ? 'הטופס לא נמצא' : err.title }}</p>
          <a class="btn" routerLink="/forms">חזרה לרשימה</a>
        </div>
      } @else if (template(); as form) {
        <div class="head">
          <a class="icon-btn" routerLink="/forms" aria-label="חזרה לרשימה">
            <app-icon class="rtl-flip" name="arrow-back" [size]="17" />
          </a>
          <h1>{{ form.name }}</h1>
          <span class="badge" [class.badge--published]="form.status === 'Published'"
                [class.badge--draft]="form.status === 'Draft'">
            {{ statusLabel(form.status) }}
          </span>
        </div>
        <p class="meta">
          נוצר על ידי <span dir="auto">{{ form.createdBy }}</span>
          &nbsp;·&nbsp; <span dir="ltr">{{ form.createdAtUtc | date: 'dd/MM/yyyy' }}</span>
          &nbsp;·&nbsp; {{ form.fields.length }} שדות
          &nbsp;·&nbsp; {{ form.approvalSteps.length }} שלבי אישור
        </p>

        <div class="layout">
          <section class="card">
            <div class="card__head">
              <h2 class="card__title">טופס</h2>
              <span class="muted" style="font-size: 12px">לקריאה בלבד</span>
            </div>
            <div class="card__body">
              <app-form-preview [fields]="previewFields()" [steps]="previewSteps()" />
            </div>
          </section>
        </div>
      }
    </div>
  `,
})
export class FormViewPage {
  private readonly api = inject(FormTemplateApiService);

  readonly id = input.required<string>();

  readonly template = signal<FormTemplateDetail | null>(null);
  readonly loading = signal(true);
  readonly error = signal<ApiError | null>(null);

  readonly previewFields = computed<PreviewField[]>(
    () =>
      this.template()?.fields.map((field) => ({
        label: field.label,
        fieldType: field.fieldType,
        isRequired: field.isRequired,
        placeholder: field.placeholder,
      })) ?? [],
  );

  readonly previewSteps = computed<PreviewStep[]>(
    () =>
      this.template()?.approvalSteps.map((step) => ({
        name: step.name,
        actionType: step.actionType,
        approverId: step.approverId,
      })) ?? [],
  );

  constructor() {
    effect(() => this.load(this.id()));
  }

  protected statusLabel(status: TemplateStatus): string {
    return status === 'Published' ? 'פורסם' : 'טיוטה';
  }

  private load(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getById(id).subscribe({
      next: (template) => {
        this.template.set(template);
        this.loading.set(false);
      },
      error: (error: ApiError) => {
        this.error.set(error);
        this.loading.set(false);
      },
    });
  }
}
