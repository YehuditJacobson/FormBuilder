import { HttpErrorResponse } from '@angular/common/http';

/**
 * A normalised view of a failed HTTP call. The error interceptor converts every
 * `HttpErrorResponse` (including RFC 7807 `ProblemDetails` / `ValidationProblemDetails`
 * bodies from the API) into this shape so components handle one type.
 */
export interface ApiError {
  status: number;
  /** Short summary, safe to show to the user. */
  title: string;
  /** Longer explanation when the server provided one. */
  detail: string | null;
  /** Field name -> messages, from a `ValidationProblemDetails` body. Empty when not a validation error. */
  fieldErrors: Record<string, string[]>;
}

interface ProblemDetailsBody {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export function toApiError(response: HttpErrorResponse): ApiError {
  // Network / CORS / offline: status 0, no useful body.
  if (response.status === 0) {
    return {
      status: 0,
      title: 'לא ניתן להתחבר לשרת',
      detail: null,
      fieldErrors: {},
    };
  }

  const body: ProblemDetailsBody | string | null =
    typeof response.error === 'object' ? (response.error as ProblemDetailsBody) : null;

  return {
    status: response.status,
    title: body?.title ?? defaultTitle(response.status),
    detail: body?.detail ?? null,
    fieldErrors: body?.errors ?? {},
  };
}

function defaultTitle(status: number): string {
  switch (status) {
    case 400:
      return 'הבקשה אינה תקינה';
    case 404:
      return 'הפריט לא נמצא';
    case 409:
      return 'הפעולה מתנגשת עם מצב קיים';
    default:
      return status >= 500 ? 'אירעה שגיאה בשרת' : 'הבקשה נכשלה';
  }
}
