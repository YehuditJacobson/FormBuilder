import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { FormTemplateApiService } from '../../../core/api/form-template-api.service';
import { FormTemplateSummary } from '../../../core/models/form-template.models';
import { FormListPage } from './form-list.page';

describe('FormListPage', () => {
  const api = { list: vi.fn() };

  const summary = (over: Partial<FormTemplateSummary> = {}): FormTemplateSummary => ({
    id: '1',
    name: 'בקשת חופשה',
    description: null,
    createdAtUtc: '2026-01-12T00:00:00Z',
    createdBy: 'דנה לוי',
    status: 'Published',
    fieldCount: 6,
    approvalStepCount: 3,
    ...over,
  });

  beforeEach(async () => {
    api.list.mockReset();
    await TestBed.configureTestingModule({
      imports: [FormListPage],
      providers: [provideRouter([]), { provide: FormTemplateApiService, useValue: api }],
    }).compileComponents();
  });

  it('shows the empty state when there are no templates', () => {
    api.list.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(FormListPage);
    fixture.detectChanges();
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('עדיין אין תבניות טפסים');
  });

  it('renders one row per template', () => {
    api.list.mockReturnValue(of([summary(), summary({ id: '2', name: 'דיווח ימי מחלה', status: 'Draft' })]));

    const fixture = TestBed.createComponent(FormListPage);
    fixture.detectChanges();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(element.textContent).toContain('בקשת חופשה');
    expect(element.textContent).toContain('פורסם');
    expect(element.textContent).toContain('טיוטה');
  });
});
