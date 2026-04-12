import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { GlobalErrorService } from '../error/global-error.service';

function problemMessage(err: HttpErrorResponse): string {
  const body = err.error;
  if (body && typeof body === 'object') {
    if (typeof (body as { detail?: string }).detail === 'string') {
      return (body as { detail: string }).detail;
    }
    if (typeof (body as { title?: string }).title === 'string') {
      return (body as { title: string }).title;
    }
    if (typeof (body as { message?: string }).message === 'string') {
      return (body as { message: string }).message;
    }
  }
  return err.message || `Request failed (${err.status})`;
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const errors = inject(GlobalErrorService);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse) {
        if (err.status === 401) {
          const isAuthForm =
            req.url.includes('/identity/login') || req.url.includes('/identity/register');
          if (isAuthForm) {
            errors.set(problemMessage(err));
          } else {
            auth.logout();
            errors.set('Session expired. Please sign in again.');
          }
        } else {
          errors.set(problemMessage(err));
        }
      }
      return throwError(() => err);
    }),
  );
};
