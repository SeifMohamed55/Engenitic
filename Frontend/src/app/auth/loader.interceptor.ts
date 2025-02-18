import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { delay, finalize } from 'rxjs';


export const loaderInterceptor: HttpInterceptorFn = (req, next) => {
  const spinner = inject(NgxSpinnerService);
  spinner.show();
  return next(req).pipe(
    finalize( () => {
      setTimeout(() => {
        spinner.hide();
      }, 500);
    })
  );
};
