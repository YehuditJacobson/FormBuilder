import { BuilderValue } from './builder-form';
import { toCreateRequest } from './builder-form.mapper';

describe('toCreateRequest', () => {
  const value: BuilderValue = {
    name: '  בקשת חופשה  ',
    description: '   ',
    fields: [
      { label: '  שם העובד  ', fieldType: 'Text', isRequired: true, placeholder: '  דנה  ' },
      { label: 'תאריך התחלה', fieldType: 'Date', isRequired: false, placeholder: '' },
    ],
    approvalSteps: [
      { name: '  מנהל  ', actionType: 'Approve', approverId: '' },
      { name: 'משאבי אנוש', actionType: 'Sign', approverId: '  hr@tax.gov.il  ' },
    ],
  };

  it('trims text and turns blanks into null', () => {
    const request = toCreateRequest(value);

    expect(request.name).toBe('בקשת חופשה');
    expect(request.description).toBeNull();
    expect(request.fields[0]).toEqual({
      label: 'שם העובד',
      fieldType: 'Text',
      isRequired: true,
      placeholder: 'דנה',
      options: null,
    });
    expect(request.fields[1].placeholder).toBeNull();
    expect(request.approvalSteps[0].approverId).toBeNull();
    expect(request.approvalSteps[1].approverId).toBe('hr@tax.gov.il');
  });

  it('preserves field and step order', () => {
    const request = toCreateRequest(value);

    expect(request.fields.map((field) => field.label)).toEqual(['שם העובד', 'תאריך התחלה']);
    expect(request.approvalSteps.map((step) => step.name)).toEqual(['מנהל', 'משאבי אנוש']);
  });
});
