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
  let myToken: string | null = null;
  // Get the token from localStorage
  if (typeof localStorage !== 'undefined') {
    myToken = localStorage.getItem('Token');
  }

  // If token exists, add it to the request headers
  if (myToken) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${myToken}`
      }
    });
  }
  else {
    console.warn('No token found in localStorage');
  }
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        return _UserService.refreshToken().pipe(
          switchMap((res: any) => {
            const newToken = res.data?.accessToken;
            if (!newToken) {
              console.error('Failed to get new access token');
              _ToastrService.error("session expired !");
              _UserService.registered.next('');
              _Router.navigate(['/login']);
              return throwError(() => new Error('Failed to refresh token'));
            }
            // Store new token and retry request
            localStorage.setItem('Token', newToken);
            const newReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${newToken}`
              }
            });
            return next(newReq);
          }),
          catchError((refreshError: any) => {
            console.error('Token refresh failed:', refreshError);
            // Perform logout and clear storage
            return _UserService.logoutConfirmation().pipe(
              switchMap(() => {
                localStorage.clear();
                _ToastrService.error("session expired !");
                _UserService.registered.next('');
                _Router.navigate(['/login']);
                return throwError(() => refreshError);
              }),
              catchError((logoutError) => {
                console.error('Logout failed:', logoutError);
                _UserService.registered.next('');
                _ToastrService.error("something went wrong !");
                return throwError(() => logoutError);
              })
            );
          })
        );
      }
      else if(error.status === 403) {
        console.log("forbiden error");
        return throwError(() => error);
      }
      else {
        return throwError(() => error);
      }
    })
  );
};
