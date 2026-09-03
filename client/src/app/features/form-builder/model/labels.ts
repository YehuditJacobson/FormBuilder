import {
  APPROVAL_ACTION_TYPES,
  ApprovalActionType,
  FieldType,
} from '../../../core/models/form-template.models';
import { IconName } from '../../../shared/ui/icon/icon.component';

export const FIELD_TYPE_LABELS: Record<FieldType, string> = {
  Text: 'טקסט',
  Date: 'תאריך',
  Number: 'מספר',
  Checkbox: 'תיבת סימון',
  Dropdown: 'רשימה נפתחת',
};

export const APPROVAL_ACTION_LABELS: Record<ApprovalActionType, string> = {
  Approve: 'אישור',
  Reject: 'דחייה',
  ReturnForRevision: 'החזרה לתיקון',
  Sign: 'חתימה',
  Acknowledge: 'יידוע',
};

export const APPROVAL_ACTION_OPTIONS: { value: ApprovalActionType; label: string }[] =
  APPROVAL_ACTION_TYPES.map((value) => ({ value, label: APPROVAL_ACTION_LABELS[value] }));

export interface AddFieldButton {
  type: FieldType;
  label: string;
  icon: IconName;
}

export const ADD_FIELD_BUTTONS: AddFieldButton[] = [
  { type: 'Text', label: 'שדה טקסט', icon: 'text' },
  { type: 'Date', label: 'שדה תאריך', icon: 'calendar' },
  { type: 'Number', label: 'מספר', icon: 'hash' },
  { type: 'Checkbox', label: 'תיבת סימון', icon: 'checkbox' },
  { type: 'Dropdown', label: 'רשימה נפתחת', icon: 'list' },
];
