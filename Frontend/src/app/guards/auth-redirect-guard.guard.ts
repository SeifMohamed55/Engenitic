import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../feature/users/user.service';
import { map, take } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

export const authRedirectGuardGuard: CanActivateFn = (route, state) => {
  const _UserService = inject(UserService);
  const router = inject(Router);
  const toastr = inject(ToastrService);
  return _UserService.role.pipe(
    take(1), // only take the current value
    map((role) => {
      if (role) {
        router.navigate(['/home']);
        toastr.warning("can't reach this page unless you are not logged in")
        return false;
      }
      return true;
    })
  );
};
