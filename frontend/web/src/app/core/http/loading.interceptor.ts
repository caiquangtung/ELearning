import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from '../loading/loading.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loading = inject(LoadingService);

  // Skip static assets (and similar) if any leak through HttpClient.
  if (req.url.endsWith('.svg') || req.url.endsWith('.png') || req.url.endsWith('.webp')) {
    return next(req);
  }

  loading.start();
  return next(req).pipe(finalize(() => loading.stop()));
};

