/**
 * TypeScript mirrors of the API DTOs (`FormBuilder.Application.FormTemplates.Contracts`).
 * Enums arrive as strings because the API uses `JsonStringEnumConverter`.
 */

export const FIELD_TYPES = ['Text', 'Date', 'Number', 'Checkbox', 'Dropdown'] as const;
export type FieldType = (typeof FIELD_TYPES)[number];

export const APPROVAL_ACTION_TYPES = [
  'Approve',
  'Reject',
  'ReturnForRevision',
  'Sign',
  'Acknowledge',
] as const;
export type ApprovalActionType = (typeof APPROVAL_ACTION_TYPES)[number];

export type TemplateStatus = 'Draft' | 'Published';

// ---- request (POST /api/v1/forms) ----

export interface CreateFormFieldInput {
  label: string;
  fieldType: FieldType;
  isRequired: boolean;
  placeholder: string | null;
  options: string | null;
}

export interface CreateApprovalStepInput {
  name: string;
  actionType: ApprovalActionType;
  approverId: string | null;
}

export interface CreateFormTemplateRequest {
  name: string;
  description: string | null;
  fields: CreateFormFieldInput[];
  approvalSteps: CreateApprovalStepInput[];
}

export interface CreateFormTemplateResponse {
  id: string;
}

// ---- responses (GET) ----

export interface FormTemplateSummary {
  id: string;
  name: string;
  description: string | null;
  createdAtUtc: string;
  createdBy: string;
  status: TemplateStatus;
  fieldCount: number;
  approvalStepCount: number;
}

export interface FormFieldDto {
  id: string;
  label: string;
  fieldType: FieldType;
  order: number;
  isRequired: boolean;
  placeholder: string | null;
  options: string | null;
}

export interface ApprovalStepDto {
  id: string;
  order: number;
  name: string;
  approverId: string | null;
  actionType: ApprovalActionType;
}

export interface FormTemplateDetail {
  id: string;
  name: string;
  description: string | null;
  createdAtUtc: string;
  createdBy: string;
  status: TemplateStatus;
  fields: FormFieldDto[];
  approvalSteps: ApprovalStepDto[];
}
