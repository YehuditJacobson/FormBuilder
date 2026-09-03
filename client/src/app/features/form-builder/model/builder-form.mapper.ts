import {
  CreateApprovalStepInput,
  CreateFormFieldInput,
  CreateFormTemplateRequest,
} from '../../../core/models/form-template.models';
import { BuilderValue } from './builder-form';

function trimToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

/** Turns the raw builder-form value into the API request. Order comes from array position. */
export function toCreateRequest(value: BuilderValue): CreateFormTemplateRequest {
  return {
    name: value.name.trim(),
    description: trimToNull(value.description),
    fields: value.fields.map(
      (field): CreateFormFieldInput => ({
        label: field.label.trim(),
        fieldType: field.fieldType,
        isRequired: field.isRequired,
        placeholder: trimToNull(field.placeholder),
        options: null,
      }),
    ),
    approvalSteps: value.approvalSteps.map(
      (step): CreateApprovalStepInput => ({
        name: step.name.trim(),
        actionType: step.actionType,
        approverId: trimToNull(step.approverId),
      }),
    ),
  };
}
