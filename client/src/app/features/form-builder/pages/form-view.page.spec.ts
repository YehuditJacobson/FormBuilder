import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { FormTemplateApiService } from '../../../core/api/form-template-api.service';
import { FormTemplateDetail } from '../../../core/models/form-template.models';
import { FormViewPage } from './form-view.page';

describe('FormViewPage', () => {
  const api = { getById: vi.fn() };

  const detail: FormTemplateDetail = {
    id: 'abc',
    name: 'בקשת חופשה',
    description: null,
    createdAtUtc: '2026-01-12T00:00:00Z',
    createdBy: 'דנה לוי',
    status: 'Published',
    fields: [
      { id: 'f1', label: 'שם העובד', fieldType: 'Text', order: 0, isRequired: true, placeholder: null, options: null },
    ],
    approvalSteps: [
      { id: 's1', order: 0, name: 'אישור מנהל', approverId: null, actionType: 'Approve' },
    ],
  };

  beforeEach(async () => {
    api.getById.mockReset();
    await TestBed.configureTestingModule({
      imports: [FormViewPage],
      providers: [provideRouter([]), { provide: FormTemplateApiService, useValue: api }],
    }).compileComponents();
  });

  function render(id: string): ComponentFixture<FormViewPage> {
    const fixture = TestBed.createComponent(FormViewPage);
    fixture.componentRef.setInput('id', id);
    fixture.detectChanges();
    fixture.detectChanges();
    return fixture;
  }

  it('loads and renders the template detail', () => {
    api.getById.mockReturnValue(of(detail));

    const element = render('abc').nativeElement as HTMLElement;

    expect(api.getById).toHaveBeenCalledWith('abc');
    expect(element.textContent).toContain('בקשת חופשה');
    expect(element.textContent).toContain('שם העובד');
    expect(element.textContent).toContain('פורסם');
  });

  it('shows a not-found message on 404', () => {
    api.getById.mockReturnValue(
      throwError(() => ({ status: 404, title: 'x', detail: null, fieldErrors: {} })),
    );

    const element = render('missing').nativeElement as HTMLElement;

    expect(element.textContent).toContain('הטופס לא נמצא');
  });
});
