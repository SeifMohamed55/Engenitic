import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { UserService } from '../feature/users/user.service';
import { catchError, switchMap, tap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

export const headerInterceptor: HttpInterceptorFn = (req, next) => {
  const _UserService = inject(UserService);
  const _Router = inject(Router);
  const _ToastrService = inject(ToastrService);
  let token!: string;

  if (typeof localStorage !== 'undefined') {
    token = localStorage.getItem('Token') || '';
  }

  req = req.clone({
    withCredentials: true, // Always include credentials
    setHeaders: token ? { Authorization: `Bearer ${token}` } : {}, // Add token only if it exists
  });

  let retryCount = 0;
  const maxRetries = 1;

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        if (_Router.url === '/login') {
          return throwError(() => error);
        }

        return _UserService.refreshToken().pipe(
          switchMap((res: any) => {
            const newToken = res.data?.accessToken;
            if (!newToken) {
              console.error('Failed to get new access token');
              _ToastrService.error('Session expired!');
              _UserService.registered.next('');
              localStorage.clear();
              if (_Router.url !== '/login') {
                _Router.navigate(['/login']);
              }
              return throwError(() => new Error('Failed to refresh token'));
            }

            // Store new token and retry request
            localStorage.setItem('Token', newToken);
            const newReq = req.clone({
              setHeaders: { Authorization: `Bearer ${newToken}` },
            });

            return retryCount < maxRetries
              ? (retryCount++, next(newReq))
              : throwError(() => new Error('Max retries reached'));
          }),
          catchError((error: HttpErrorResponse) => {
            return _UserService.logoutConfirmation().pipe(
              tap(() => {
                localStorage.clear();
                _UserService.registered.next('');
                _UserService.role.next('');
                _ToastrService.error('Session expired!');
                if (_Router.url !== '/login') {
                  _Router.navigate(['/login']);
                }
                return throwError(() => error);
              }),
              catchError((error: HttpErrorResponse) => {
                localStorage.clear();
                _UserService.registered.next('');
                _UserService.role.next('');
                if (_Router.url !== '/login') {
                  _Router.navigate(['/login']);
                }
                return throwError(() => error);
              })
            );
          })
        );
      } else if (error.status === 403) {
        return throwError(() => error);
      } else {
        return throwError(() => error);
      }
    })
  );
};
