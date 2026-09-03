import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { toApiError } from '../errors/api-error';

/**
 * Converts every failed response into a normalised {@link ApiError} before it reaches a
 * component, so callers never branch on raw `HttpErrorResponse` shapes.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: HttpErrorResponse) => throwError(() => toApiError(error))),
  );
