import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormArray, NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { map } from 'rxjs';
import { FormTemplateApiService } from '../../../core/api/form-template-api.service';
import { ApiError } from '../../../core/errors/api-error';
import { FieldType } from '../../../core/models/form-template.models';
import { IconComponent } from '../../../shared/ui/icon/icon.component';
import { ApprovalStepRowComponent } from '../components/approval-step-row.component';
import { FieldRowComponent } from '../components/field-row.component';
import {
  FormPreviewComponent,
  PreviewField,
  PreviewStep,
} from '../components/form-preview.component';
import {
  BuilderForm,
  FieldGroup,
  StepGroup,
  createBuilderForm,
  newFieldGroup,
  newStepGroup,
} from '../model/builder-form';
import { ADD_FIELD_BUTTONS } from '../model/labels';
import { toCreateRequest } from '../model/builder-form.mapper';

@Component({
  selector: 'app-form-builder-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    IconComponent,
    FieldRowComponent,
    ApprovalStepRowComponent,
    FormPreviewComponent,
  ],
  templateUrl: './form-builder.page.html',
  styleUrl: './form-builder.page.scss',
})
export class FormBuilderPage {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(FormTemplateApiService);
  private readonly router = inject(Router);

  readonly form: BuilderForm = createBuilderForm(this.fb);
  readonly addFieldButtons = ADD_FIELD_BUTTONS;

  readonly saving = signal(false);
  readonly serverError = signal<ApiError | null>(null);

  readonly value = toSignal(
    this.form.valueChanges.pipe(map(() => this.form.getRawValue())),
    { initialValue: this.form.getRawValue() },
  );

  readonly previewFields = computed<PreviewField[]>(() =>
    this.value().fields.map((field) => ({
      label: field.label,
      fieldType: field.fieldType,
      isRequired: field.isRequired,
      placeholder: field.placeholder.trim() || null,
    })),
  );

  readonly previewSteps = computed<PreviewStep[]>(() =>
    this.value().approvalSteps.map((step) => ({
      name: step.name,
      actionType: step.actionType,
      approverId: step.approverId.trim() || null,
    })),
  );

  readonly hasStarted = computed(() => {
    const current = this.value();
    return current.name.trim().length > 0 || current.fields.length > 0 || current.approvalSteps.length > 0;
  });

  readonly errorSummary = computed<string[]>(() => {
    this.value(); // re-evaluate on any form change
    const messages: string[] = [];

    if (this.form.controls.name.invalid) {
      messages.push('שם הטופס הוא שדה חובה');
    }
    if (this.fields.length === 0) {
      messages.push('יש להוסיף לפחות שדה אחד');
    }
    this.fields.controls.forEach((group, index) => {
      if (group.controls.label.invalid) {
        messages.push(`לשדה ${index + 1} חסרה תווית`);
      }
    });
    if (this.steps.length === 0) {
      messages.push('יש להוסיף לפחות שלב אישור אחד');
    }
    this.steps.controls.forEach((group, index) => {
      if (group.controls.name.invalid) {
        messages.push(`לשלב אישור ${index + 1} חסר שם`);
      }
    });

    return messages;
  });

  get fields(): FormArray<FieldGroup> {
    return this.form.controls.fields;
  }

  get steps(): FormArray<StepGroup> {
    return this.form.controls.approvalSteps;
  }

  addField(type: FieldType): void {
    this.fields.push(newFieldGroup(this.fb, type));
  }

  removeField(index: number): void {
    this.fields.removeAt(index);
  }

  moveField(from: number, to: number): void {
    if (to < 0 || to >= this.fields.length) {
      return;
    }
    const control = this.fields.at(from);
    this.fields.removeAt(from);
    this.fields.insert(to, control);
  }

  addStep(): void {
    this.steps.push(newStepGroup(this.fb));
  }

  removeStep(index: number): void {
    this.steps.removeAt(index);
  }

  moveStep(from: number, to: number): void {
    if (to < 0 || to >= this.steps.length) {
      return;
    }
    const control = this.steps.at(from);
    this.steps.removeAt(from);
    this.steps.insert(to, control);
  }

  submit(): void {
    this.serverError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.api.create(toCreateRequest(this.form.getRawValue())).subscribe({
      next: ({ id }) => {
        this.saving.set(false);
        void this.router.navigate(['/forms', id]);
      },
      error: (error: ApiError) => {
        this.saving.set(false);
        this.serverError.set(error);
      },
    });
  }

  cancel(): void {
    void this.router.navigateByUrl('/forms');
  }
}
