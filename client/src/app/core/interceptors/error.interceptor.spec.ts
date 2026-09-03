import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('maps a ValidationProblemDetails body into ApiError.fieldErrors', async () => {
    const promise = firstValueFrom(http.get('/api/v1/forms'));

    httpMock.expectOne('/api/v1/forms').flush(
      { title: 'Validation failed', errors: { 'Request.Name': ['Form name is required.'] } },
      { status: 400, statusText: 'Bad Request' },
    );

    await expect(promise).rejects.toMatchObject({
      status: 400,
      title: 'Validation failed',
      fieldErrors: { 'Request.Name': ['Form name is required.'] },
    });
  });

  it('maps a network failure (status 0) to a connection error', async () => {
    const promise = firstValueFrom(http.get('/api/v1/forms'));

    httpMock.expectOne('/api/v1/forms').error(new ProgressEvent('error'), { status: 0 });

    await expect(promise).rejects.toMatchObject({ status: 0, fieldErrors: {} });
  });
});
