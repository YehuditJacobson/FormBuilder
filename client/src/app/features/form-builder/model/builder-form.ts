import { FormArray, FormControl, FormGroup, NonNullableFormBuilder, Validators } from '@angular/forms';
import { ApprovalActionType, FieldType } from '../../../core/models/form-template.models';
import { minLengthArray } from './builder-form.validators';

const MAX_NAME = 200;
const MAX_DESCRIPTION = 1000;
const MAX_PLACEHOLDER = 200;
const MAX_APPROVER = 200;

export type FieldGroup = FormGroup<{
  label: FormControl<string>;
  fieldType: FormControl<FieldType>;
  isRequired: FormControl<boolean>;
  placeholder: FormControl<string>;
}>;

export type StepGroup = FormGroup<{
  name: FormControl<string>;
  actionType: FormControl<ApprovalActionType>;
  approverId: FormControl<string>;
}>;

export type BuilderForm = FormGroup<{
  name: FormControl<string>;
  description: FormControl<string>;
  fields: FormArray<FieldGroup>;
  approvalSteps: FormArray<StepGroup>;
}>;

export type BuilderValue = ReturnType<BuilderForm['getRawValue']>;

export function createBuilderForm(fb: NonNullableFormBuilder): BuilderForm {
  return fb.group({
    name: fb.control('', [Validators.required, Validators.maxLength(MAX_NAME)]),
    description: fb.control('', [Validators.maxLength(MAX_DESCRIPTION)]),
    fields: fb.array<FieldGroup>([], minLengthArray(1)),
    approvalSteps: fb.array<StepGroup>([], minLengthArray(1)),
  });
}

export function newFieldGroup(fb: NonNullableFormBuilder, fieldType: FieldType): FieldGroup {
  return fb.group({
    label: fb.control('', [Validators.required, Validators.maxLength(MAX_NAME)]),
    fieldType: fb.control(fieldType),
    isRequired: fb.control(false),
    placeholder: fb.control('', [Validators.maxLength(MAX_PLACEHOLDER)]),
  });
}

export function newStepGroup(fb: NonNullableFormBuilder): StepGroup {
  return fb.group({
    name: fb.control('', [Validators.required, Validators.maxLength(MAX_NAME)]),
    actionType: fb.control<ApprovalActionType>('Approve'),
    approverId: fb.control('', [Validators.maxLength(MAX_APPROVER)]),
  });
}
