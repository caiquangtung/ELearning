import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { Roles } from './roles';

function defaultPortal(): string {
  const roles = inject(AuthService).user()?.roles ?? [];
  if (roles.includes(Roles.Admin)) return '/admin';
  if (roles.includes(Roles.Instructor)) return '/teach';
  return '/learn';
}

function ensureAuthenticated(): true | ReturnType<Router['createUrlTree']> {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated()) return true;
  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: router.url },
  });
}

function portalForLearningResource(): string {
  const roles = inject(AuthService).user()?.roles ?? [];
  if (roles.includes(Roles.Admin) || roles.includes(Roles.Instructor)) {
    return '/teach';
  }
  return '/learn';
}

export const defaultPortalRedirectGuard: CanActivateFn = () => {
  const authenticated = ensureAuthenticated();
  if (authenticated !== true) return authenticated;
  return inject(Router).createUrlTree([defaultPortal()]);
};

export const legacyCoursesRedirectGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
) => {
  const authenticated = ensureAuthenticated();
  if (authenticated !== true) return authenticated;
  const id = route.paramMap.get('id');
  const target = id
    ? `${portalForLearningResource()}/courses/${id}`
    : `${portalForLearningResource()}/courses`;
  return inject(Router).createUrlTree([target], {
    queryParams: route.queryParams,
  });
};

export const legacyClassesRedirectGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
) => {
  const authenticated = ensureAuthenticated();
  if (authenticated !== true) return authenticated;
  const id = route.paramMap.get('id');
  const target = id
    ? `${portalForLearningResource()}/classes/${id}`
    : `${portalForLearningResource()}/classes`;
  return inject(Router).createUrlTree([target], {
    queryParams: route.queryParams,
  });
};

export const legacyOrdersRedirectGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
) => {
  const authenticated = ensureAuthenticated();
  if (authenticated !== true) return authenticated;
  const id = route.paramMap.get('id');
  return inject(Router).createUrlTree([id ? `/learn/orders/${id}` : '/learn/orders'], {
    queryParams: route.queryParams,
  });
};

export const legacyCheckoutRedirectGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
) => {
  const authenticated = ensureAuthenticated();
  if (authenticated !== true) return authenticated;
  return inject(Router).createUrlTree(['/learn/checkout'], {
    queryParams: route.queryParams,
  });
};

export const legacyNotificationsRedirectGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
) => {
  const authenticated = ensureAuthenticated();
  if (authenticated !== true) return authenticated;
  return inject(Router).createUrlTree([`${defaultPortal()}/notifications`], {
    queryParams: route.queryParams,
  });
};

export const legacyQuizzesRedirectGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
) => {
  const authenticated = ensureAuthenticated();
  if (authenticated !== true) return authenticated;
  const child = route.firstChild;
  const tail = child?.url.map((segment) => segment.path).join('/') ?? '';
  const target = tail ? `/teach/${tail}` : '/teach/quizzes';
  return inject(Router).createUrlTree([target], {
    queryParams: route.queryParams,
  });
};

export const legacyAdminRedirectGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
) => {
  const authenticated = ensureAuthenticated();
  if (authenticated !== true) return authenticated;
  const first = route.url[0]?.path ?? '';
  const id = route.paramMap.get('id');
  const target = id ? `/admin/${first}/${id}` : `/admin/${first}`;
  return inject(Router).createUrlTree([target], {
    queryParams: route.queryParams,
  });
};
