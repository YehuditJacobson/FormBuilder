import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateFormTemplateRequest } from '../models/form-template.models';
import { FormTemplateApiService } from './form-template-api.service';

describe('FormTemplateApiService', () => {
  let service: FormTemplateApiService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiBaseUrl}/v1/forms`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(FormTemplateApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('list() issues GET /v1/forms', async () => {
    const promise = firstValueFrom(service.list());

    const request = httpMock.expectOne(base);
    expect(request.request.method).toBe('GET');
    request.flush([]);

    await expect(promise).resolves.toEqual([]);
  });

  it('getById() appends the id to the path', async () => {
    const promise = firstValueFrom(service.getById('abc-123'));

    const request = httpMock.expectOne(`${base}/abc-123`);
    expect(request.request.method).toBe('GET');
    request.flush({ id: 'abc-123' });

    await promise;
  });

  it('create() posts the request body', async () => {
    const body: CreateFormTemplateRequest = {
      name: 'Vacation Request',
      description: null,
      fields: [],
      approvalSteps: [],
    };
    const promise = firstValueFrom(service.create(body));

    const request = httpMock.expectOne(base);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ id: 'new-id' });

    await expect(promise).resolves.toEqual({ id: 'new-id' });
  });
});
