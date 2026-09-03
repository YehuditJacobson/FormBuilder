import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateFormTemplateRequest,
  CreateFormTemplateResponse,
  FormTemplateDetail,
  FormTemplateSummary,
} from '../models/form-template.models';

/** Typed wrapper over the three form-template endpoints. */
@Injectable({ providedIn: 'root' })
export class FormTemplateApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/v1/forms`;

  list(): Observable<FormTemplateSummary[]> {
    return this.http.get<FormTemplateSummary[]>(this.baseUrl);
  }

  getById(id: string): Observable<FormTemplateDetail> {
    return this.http.get<FormTemplateDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateFormTemplateRequest): Observable<CreateFormTemplateResponse> {
    return this.http.post<CreateFormTemplateResponse>(this.baseUrl, request);
  }
}
