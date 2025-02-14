import { HttpErrorResponse, HttpHandler, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { UserService } from '../feature/users/user.service';
import { catchError } from 'rxjs';

export const headerInterceptor: HttpInterceptorFn = (req, next) => {

  const _UserService = inject(UserService);
  
  const myToken : any = localStorage.getItem('Token');
  
  req = req.clone({
    setHeaders : {
      token : myToken
    }
  });

  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) =>
      {
        if (error.status === 401){
          _UserService.refreshToken().subscribe({
            next : res =>{
              
            },
            error : err =>{

            }
          })
        }
    })
  );
};