import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { UserService } from '../feature/users/user.service';
import { catchError, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

export const headerInterceptor: HttpInterceptorFn = (req, next) => {
  const _UserService = inject(UserService);
  const _Router = inject(Router);
  const _ToastrService = inject(ToastrService);

  const token =
    typeof localStorage !== 'undefined'
      ? localStorage.getItem('Token') || ''
      : '';

  // Clone request with token (if exists) and credentials
  req = req.clone({
    withCredentials: true,
    setHeaders: token ? { Authorization: `Bearer ${token}` } : {},
  });

  const forceLogout = () => {
    localStorage.clear();
    _UserService.registered.next('');
    _UserService.role.next('');
    _ToastrService.error('Session expired!');
    if (_Router.url !== '/login') {
      _Router.navigate(['/login']);
    }
  };

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Don't retry if already on login page
        if (_Router.url === '/login') {
          return throwError(() => error);
        }

        // Attempt to refresh token
        return _UserService.refreshToken().pipe(
          switchMap((res: any) => {
            const newToken = res?.data?.accessToken;
            if (!newToken) {
              forceLogout();
              return throwError(() => new Error('Failed to refresh token'));
            }

            // Save new token and retry the original request
            localStorage.setItem('Token', newToken);
            const newReq = req.clone({
              setHeaders: { Authorization: `Bearer ${newToken}` },
            });

            return next(newReq);
          }),
          catchError(() => {
            // If refresh failed, ask user for logout confirmation
            return _UserService.logoutConfirmation().pipe(
              switchMap(() => {
                forceLogout();
                return throwError(
                  () => new Error('Session expired after refresh attempt')
                );
              }),
              catchError(() => {
                forceLogout();
                return throwError(
                  () => new Error('Session expired and logout failed')
                );
              })
            );
          })
        );
      } else if (error.status === 403) {
        // Forbidden - likely invalid permissions
        return throwError(() => error);
      } else {
        // Other errors - forward them
        return throwError(() => error);
      }
    })
  );
};
