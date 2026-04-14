import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorResponse } from '../models/api-error.model';

export function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const payload = error.error as ApiErrorResponse | string | null;

  if (typeof payload === 'string' && payload.trim()) {
    return payload;
  }

  if (payload && typeof payload === 'object') {
    const validationMessages = Object.values(payload.errors ?? {})
      .flat()
      .filter((message) => !!message);

    if (validationMessages.length > 0) {
      return validationMessages[0];
    }

    if (payload.detail?.trim()) {
      return payload.detail;
    }

    if (payload.title?.trim()) {
      return payload.title;
    }
  }

  return fallback;
}
