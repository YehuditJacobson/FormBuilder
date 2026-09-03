import { provideHttpClient } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { FormTemplateApiService } from '../../../core/api/form-template-api.service';
import { FormBuilderPage } from './form-builder.page';

describe('FormBuilderPage', () => {
  let fixture: ComponentFixture<FormBuilderPage>;
  let page: FormBuilderPage;
  const api = { create: vi.fn() };

  const makeValid = (): void => {
    page.form.controls.name.setValue('בקשת חופשה');
    page.addField('Text');
    page.fields.at(0).controls.label.setValue('שם העובד');
    page.addStep();
    page.steps.at(0).controls.name.setValue('אישור מנהל');
  };

  beforeEach(async () => {
    api.create.mockReset();

    await TestBed.configureTestingModule({
      imports: [FormBuilderPage],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: FormTemplateApiService, useValue: api },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FormBuilderPage);
    page = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('starts invalid with no fields and no steps', () => {
    expect(page.form.invalid).toBe(true);
    expect(page.fields.length).toBe(0);
    expect(page.steps.length).toBe(0);
  });

  it('adds and removes fields', () => {
    page.addField('Text');
    page.addField('Date');
    expect(page.fields.length).toBe(2);

    page.removeField(0);
    expect(page.fields.length).toBe(1);
    expect(page.fields.at(0).controls.fieldType.value).toBe('Date');
  });

  it('moves a field up', () => {
    page.addField('Text');
    page.addField('Date');
    page.fields.at(0).controls.label.setValue('ראשון');
    page.fields.at(1).controls.label.setValue('שני');

    page.moveField(1, 0);

    expect(page.fields.at(0).controls.label.value).toBe('שני');
  });

  it('does not call the API while the form is invalid', () => {
    page.submit();
    expect(api.create).not.toHaveBeenCalled();
  });

  it('submits a valid form and shows the saved state', () => {
    api.create.mockReturnValue(of({ id: 'new-id' }));
    makeValid();
    expect(page.form.valid).toBe(true);

    page.submit();

    expect(api.create).toHaveBeenCalledOnce();
    expect(page.savedId()).toBe('new-id');
  });

  it('surfaces a server error and stays on the form', () => {
    api.create.mockReturnValue(
      throwError(() => ({ status: 500, title: 'שגיאת שרת', detail: null, fieldErrors: {} })),
    );
    makeValid();

    page.submit();

    expect(page.serverError()?.title).toBe('שגיאת שרת');
    expect(page.savedId()).toBeNull();
  });
});
