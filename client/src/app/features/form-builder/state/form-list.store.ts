import { Injectable, computed, inject, signal } from '@angular/core';
import { FormTemplateApiService } from '../../../core/api/form-template-api.service';
import { ApiError } from '../../../core/errors/api-error';
import { FormTemplateSummary } from '../../../core/models/form-template.models';

/** Signal-based facade over the template list: one place holds the rows, loading and error state. */
@Injectable({ providedIn: 'root' })
export class FormListStore {
  private readonly api = inject(FormTemplateApiService);

  private readonly _templates = signal<FormTemplateSummary[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<ApiError | null>(null);
  private readonly _loaded = signal(false);

  readonly templates = this._templates.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly isEmpty = computed(
    () => this._loaded() && !this._loading() && !this._error() && this._templates().length === 0,
  );

  /** Fetches the list. Call `refresh()` to force a reload after a mutation. */
  load(): void {
    if (this._loaded() || this._loading()) {
      return;
    }
    this.refresh();
  }

  refresh(): void {
    this._loading.set(true);
    this._error.set(null);
    this.api.list().subscribe({
      next: (templates) => {
        this._templates.set(templates);
        this._loading.set(false);
        this._loaded.set(true);
      },
      error: (error: ApiError) => {
        this._error.set(error);
        this._loading.set(false);
        this._loaded.set(true);
      },
    });
  }
}
