import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { Roles } from './roles';

export const instructorGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login'], {
      queryParams: { returnUrl: router.url },
    });
  }

  const roles = auth.user()?.roles ?? [];
  if (roles.some((r) => r === Roles.Instructor || r === Roles.Admin))
    return true;

  return router.createUrlTree(['/learn']);
};
