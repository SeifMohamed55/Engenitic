import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { UserService } from '../feature/users/user.service';
import { catchError, switchMap, throwError } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

export const headerInterceptor: HttpInterceptorFn = (req, next) => {

  const _UserService = inject(UserService);
  const platformId = inject(PLATFORM_ID);
  
  let myToken !: string | null;

  if (isPlatformBrowser(platformId)) {
    myToken = localStorage.getItem('Token');
  }
  
  if (myToken) {
    req = req.clone({
      setHeaders : {
        Authorization : `Bearer ${myToken}`
      }
  })} 
  else {
    console.error('No token found in localStorage');
  }
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        return _UserService.refreshToken().pipe(
          switchMap((res: any) => {
            const newToken = res.data.accessToken;
            localStorage.setItem('Token', newToken);
            const newReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${newToken}`
              }
            });
            return next(newReq);
          }),
          catchError((err: any) => {
            console.error('Failed to refresh token:', err);
            _UserService.logoutConfirmation().subscribe({
              next : res => {
                localStorage.clear();
                console.log(res);
              },
              error : err =>{
                console.log(err);
              }
            });
            return throwError(() => err);
          })
        );
      } else {
        return throwError(() => error);
      }
    })
  );
};