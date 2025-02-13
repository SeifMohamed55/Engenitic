import { HttpInterceptorFn } from '@angular/common/http';

export const headerInterceptor: HttpInterceptorFn = (req, next) => {

  
  let myToken : any = {token : localStorage.getItem('Token')}

  req = req.clone({
    setHeaders : {
      token : myToken
    }
  });

  return next(req);
};
