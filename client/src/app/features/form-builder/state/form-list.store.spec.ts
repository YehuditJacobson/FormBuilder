import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { FormTemplateApiService } from '../../../core/api/form-template-api.service';
import { FormListStore } from './form-list.store';

describe('FormListStore', () => {
  const api = { list: vi.fn() };
  let store: FormListStore;

  beforeEach(() => {
    api.list.mockReset();
    TestBed.configureTestingModule({
      providers: [{ provide: FormTemplateApiService, useValue: api }],
    });
    store = TestBed.inject(FormListStore);
  });

  it('load() fetches once and fills the list', () => {
    api.list.mockReturnValue(of([{ id: '1', name: 'A' }]));

    store.load();
    store.load();

    expect(api.list).toHaveBeenCalledOnce();
    expect(store.templates()).toHaveLength(1);
    expect(store.loading()).toBe(false);
  });

  it('reports the empty state after loading nothing', () => {
    api.list.mockReturnValue(of([]));

    store.load();

    expect(store.isEmpty()).toBe(true);
  });

  it('captures an error and is not "empty"', () => {
    api.list.mockReturnValue(
      throwError(() => ({ status: 500, title: 'x', detail: null, fieldErrors: {} })),
    );

    store.load();

    expect(store.error()?.status).toBe(500);
    expect(store.isEmpty()).toBe(false);
  });

  it('refresh() reloads even after a successful load', () => {
    api.list.mockReturnValue(of([]));

    store.load();
    store.refresh();

    expect(api.list).toHaveBeenCalledTimes(2);
  });
});
